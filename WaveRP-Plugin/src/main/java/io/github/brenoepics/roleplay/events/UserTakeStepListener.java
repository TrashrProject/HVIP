package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomTileState;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.users.UserTakeStepEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.escort.EscortManager;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import java.util.Optional;

public class UserTakeStepListener implements EventListener {

  public UserTakeStepListener() {
    // empty init
  }

  private static RoomTile getFrontTileOrCurrent(UserTakeStepEvent event, Habbo habbo) {
    RoomTile toLocation = event.toLocation;
    if (event.habbo.getHabboInfo() != null && event.habbo.getHabboInfo().getCurrentRoom() != null
        && event.habbo.getHabboInfo().getCurrentRoom().getLayout() != null
        && habbo.getRoomUnit() != null && habbo.getRoomUnit().getBodyRotation() != null) {
      RoomTile frontTile = event.habbo.getHabboInfo().getCurrentRoom().getLayout()
          .getTileInFront(toLocation, habbo.getRoomUnit().getBodyRotation().getValue());
      if (frontTile != null && frontTile.state == RoomTileState.OPEN) {
        return frontTile;
      }
    }
    return toLocation;
  }

  @EventHandler
  public static void onUserTakeStep(UserTakeStepEvent event) {
    Habbo habbo = event.habbo;
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    RoomTile toLocation = getFrontTileOrCurrent(event, habbo);

    if (EscortManager.walkPrisoners(event.habbo, toLocation, event.fromLocation)) {
      return;
    }

    if (data.isDead()) {
      event.setCancelled(true);
      event.habbo.getRoomUnit().setCanWalk(false);
      return;
    }

    Optional<InventorySlot> equippedWeapon = data.getEquippedWeapon();
    if (data.isPassive() && equippedWeapon.isPresent()) {
      data.setEquippedWeapon(0);
    }

    if (canEnable(habbo.getRoomUnit().getRoom(), data) && equippedWeapon.isPresent()) {
      habbo.getRoomUnit().getRoom()
          .giveEffect(habbo, equippedWeapon.get().getItem().getEnableId(), Integer.MAX_VALUE);
    }
  }

  private static boolean canEnable(Room room, RpAvatar avatar) {
    return !avatar.isPassive() && !avatar.isDead() && room.getCategory() == Emulator.getConfig()
        .getInt("nahabbo.features.room.category") && (avatar.getEquippedWeapon().isPresent());
  }
}
