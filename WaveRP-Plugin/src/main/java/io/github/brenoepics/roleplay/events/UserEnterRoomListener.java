package io.github.brenoepics.roleplay.events;

import static io.github.brenoepics.roleplay.features.crime.prison.PrisonService.JAIL_ROOM_ID;
import static io.github.brenoepics.roleplay.features.hospital.HospitalService.HOSPITAL_ROOM_ID;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomTileState;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.users.HabboAddedToRoomEvent;
import com.eu.habbo.plugin.events.users.UserEnterRoomEvent;
import com.eu.habbo.plugin.events.users.UserIdleEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.job.JobsDelegate;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import java.util.Optional;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class UserEnterRoomListener implements EventListener {

  @EventHandler
  public static void onRoomChange(UserEnterRoomEvent event) {
    Habbo habbo = event.habbo;
    BankComputerSessionManager.disconnect(habbo);
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    RolePlay.getEscortManager().stopEscorting(habbo.getHabboInfo().getId());
    RolePlay.getEscortManager().stopEscortingByOfficer(habbo.getHabboInfo().getId());
    resetPosition(event, data);

    if (event.room.getId() != HOSPITAL_ROOM_ID) {
      RolePlay.getHospitalService().onLeaveHospital(habbo);
    }

    if (data.isPassive()) {
      data.setEquippedWeapon(0);
    }

    if (!data.isDuty() || !RolePlay.getJobsManager()
        .canWorkAtRoom(data.getJobEntity(), event.room.getId())) {
      RolePlay.getJobsManager().stopWork(event.habbo, data);
    }

    handleSpecialRooms(event, habbo, data);
    Emulator.getThreading().run(() -> {
      data.updateState();
      // Re-push the authoritative player HUD after the room transition. This keeps HP/shield,
      // username and role synchronized without requiring a browser refresh.
      data.updateLife();
      PoliceHandcuffService.enforce(habbo);
      Optional<InventorySlot> equippedWeapon = data.getEquippedWeapon();
      if (!data.isPassive() && !data.isDead() && equippedWeapon.isPresent()) {
        int visualEffect = RolePlay.getWeaponSkinService().getEquippedEffect(
            habbo.getHabboInfo().getId(),
            equippedWeapon.get().getItem().getDisplayName(),
            equippedWeapon.get().getItem().getEnableId());
        event.room.giveEffect(habbo, visualEffect, Integer.MAX_VALUE);
      }
    }, 500);
  }

  private static boolean handleSpecialRooms(UserEnterRoomEvent event, Habbo habbo, RpAvatar data) {
    if (event.room.getId() == HOSPITAL_ROOM_ID) {
      log.info("User {} entered hospital room", habbo.getHabboInfo().getUsername());
      return true;
    } else if (event.room.getId() == JAIL_ROOM_ID) {
      log.info("User {} entered jail room", habbo.getHabboInfo().getUsername());
      RolePlay.getPrisonService().onEnterJail(event, event.habbo);
      return true;

    } else if (data.isJailed() && Emulator.getIntUnixTimestamp() < data.getJailTime()) {
      log.info("User {} is jailed, sending to jail", habbo.getHabboInfo().getUsername());
      RolePlay.getPrisonHandler().sendToJailAsync(habbo);
      return false;
    } else if (data.isDead()) {
      log.info("User {} entered dead room", habbo.getHabboInfo().getUsername());
      RolePlay.getDeathHandler().sendToHospitalAsync(habbo);
      return false;
    }

    return true;
  }

  @EventHandler
  public static void onHabboAddedToRoom(HabboAddedToRoomEvent event) {
    if (event.room.getId() == HOSPITAL_ROOM_ID) {
      RolePlay.getHospitalService().onEnterHospital(event, event.habbo);
    }
  }

  private static void resetPosition(UserEnterRoomEvent event, RpAvatar data) {
    if (data.getLastPosition() == null) {
      return;
    }

    if (data.getLastPosition().getRoomId() == event.room.getId()) {
      RoomTile rt = event.room.getLayout()
          .getTile(data.getLastPosition().getX(), data.getLastPosition().getY());
      if (rt != null && rt.state != RoomTileState.INVALID && rt.state != RoomTileState.BLOCKED) {
        event.setDoorTile(rt);
        event.setRotation(RoomUserRotation.fromValue(data.getLastPosition().getRotation()));
      }
    }

    data.resetLastPosition();
  }

  //TODO: @EventHandler
  public static void onUserIdle(UserIdleEvent event) {
    if (event.habbo.getHabboInfo().getCurrentRoom() == null) {
      //TODO: This is a bug, need to add an Event in Arcturus to fix this.
      return;
    }

    if (event.habbo.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      JobsDelegate.resetLook(event.habbo);
    }
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(event.habbo);
    RolePlay.getJobsManager().stopWork(event.habbo, data);

  }
}
