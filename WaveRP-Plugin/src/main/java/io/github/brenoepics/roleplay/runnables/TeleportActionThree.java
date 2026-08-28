package io.github.brenoepics.roleplay.runnables;

import static io.github.brenoepics.roleplay.features.escort.EscortManager.onEscortList;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.interactions.InteractionTeleport;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import io.github.brenoepics.roleplay.RolePlay;
import java.util.List;
import java.util.stream.Stream;
import org.jetbrains.annotations.NotNull;

class TeleportActionThree implements Runnable {

  private final HabboItem currentTeleport;
  private final Room room;
  private final GameClient client;

  public TeleportActionThree(HabboItem currentTeleport, Room room, GameClient client) {
    this.currentTeleport = currentTeleport;
    this.client = client;
    this.room = room;
  }

  @Override
  public void run() {
    if (this.client.getHabbo().getHabboInfo().getCurrentRoom() != this.room) {
      return;
    }

    HabboItem targetTeleport;
    Room targetRoom = this.room;

    if (this.currentTeleport.getRoomId()
        != ((InteractionTeleport) this.currentTeleport).getTargetRoomId()) {
      targetRoom = Emulator.getGameEnvironment().getRoomManager()
          .loadRoom(((InteractionTeleport) this.currentTeleport).getTargetRoomId());
    }

    if (targetRoom == null) {
      Emulator.getThreading()
          .run(new TeleportActionFive(this.currentTeleport, this.room, this.client), 0);
      return;
    }

    if (targetRoom.isPreLoaded()) {
      targetRoom.loadData();
    }

    targetTeleport = targetRoom.getHabboItem(
        ((InteractionTeleport) this.currentTeleport).getTargetId());

    if (targetTeleport == null) {
      Emulator.getThreading()
          .run(new TeleportActionFive(this.currentTeleport, this.room, this.client), 0);
      return;
    }

    RoomTile teleportLocation = targetRoom.getLayout()
        .getTile(targetTeleport.getX(), targetTeleport.getY());

    if (teleportLocation == null) {
      Emulator.getThreading()
          .run(new TeleportActionFive(this.currentTeleport, this.room, this.client), 0);
      return;
    }

    if (targetRoom != this.room) {
      handleEscorted(this.client.getHabbo(), targetRoom, teleportLocation, targetTeleport);
      forwardToRoom(this.client.getHabbo(), targetRoom, teleportLocation, targetTeleport);
    }

    Emulator.getThreading().run(new TeleportActionFour(targetTeleport, targetRoom, this.client), 0);

  }

  private void forwardToRoom(Habbo habbo, Room targetRoom, RoomTile teleportLocation,
      HabboItem targetTeleport) {
    Room currentRoom = habbo.getHabboInfo().getCurrentRoom();
    if (currentRoom != null) {
      Emulator.getGameEnvironment().getRoomManager().logExit(habbo);
      currentRoom.removeHabbo(habbo, true);
      habbo.getHabboInfo().setCurrentRoom(null);
    }

    Emulator.getGameEnvironment().getRoomManager().enterRoom(habbo, targetRoom.getId(), "",
        Emulator.getConfig().getBoolean("hotel.teleport.locked.allowed"), teleportLocation,
        RoomUserRotation.values()[targetTeleport.getRotation() % 8]);
  }

  private boolean handleEscorted(Habbo habbo, Room targetRoom, RoomTile teleportLocation,
      HabboItem targetTeleport) {
    List<Integer> escorted = RolePlay.getEscortManager().getEscorted(habbo.getHabboInfo().getId());
    if (escorted == null || escorted.isEmpty()) {
      return false;
    }

    getHabboStream(habbo, escorted).forEach(
        prisoner -> forwardToRoom(prisoner, targetRoom, teleportLocation, targetTeleport));
    return true;
  }

  private static @NotNull Stream<Habbo> getHabboStream(Habbo habbo, List<Integer> escorted) {
    return habbo.getRoomUnit().getRoom().getHabbos().stream()
        .filter(h -> onEscortList(h, escorted));
  }
}
