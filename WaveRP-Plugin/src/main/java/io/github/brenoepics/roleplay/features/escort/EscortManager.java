package io.github.brenoepics.roleplay.features.escort;

import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUnitStatus;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUnitOnRollerComposer;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserStatusComposer;
import io.github.brenoepics.roleplay.RolePlay;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class EscortManager {

  public static final String ESCORT_VARIABLE = "ESCORTING_USER";
  private final HashMap<Integer, List<Integer>> escorting = new HashMap<>();

  public EscortManager() {
    /* TODO document why this constructor is empty */
  }

  public void startEscorting(int police, int prisoner) {
    escorting.computeIfAbsent(police, v -> new ArrayList<>());
    if (!escorting.get(police).contains(prisoner)) escorting.get(police).add(prisoner);
  }

  public void stopEscorting(int prisoner) {
    escorting.forEach((key, value) -> value.remove(prisoner));
  }

  public boolean isEscorted(int police) {
    return escorting.containsKey(police) && !escorting.get(police).isEmpty();
  }

  public boolean stillEscorting(int escortingPolice, int prisoner) {
    return isEscorted(escortingPolice) && escorting.get(escortingPolice).contains(prisoner);
  }

  public boolean isPrisonerEscorted(int prisoner) {
    return escorting.values().stream().anyMatch(users -> users.contains(prisoner));
  }

  public int getEscortingOfficer(int prisoner) {
    return escorting.entrySet().stream().filter(entry -> entry.getValue().contains(prisoner))
        .map(java.util.Map.Entry::getKey).findFirst().orElse(0);
  }

  public List<Integer> getEscorted(int police) {
    return escorting.get(police);
  }

  public static void walkPrisoner(Habbo prisoner, RoomTile toLocation, RoomTile fromLocation,
      int direction) {
    Room currentRoom = prisoner.getHabboInfo().getCurrentRoom();
    if (toLocation != null && toLocation.isWalkable()) {
      prisoner.getRoomUnit().resetIdleTimer();
      prisoner.getRoomUnit().setCanWalk(true);
      currentRoom.sendComposer(
          (new RoomUnitOnRollerComposer(prisoner.getRoomUnit(), toLocation, currentRoom)).compose());
    }
  }

  private static RoomTile getTileInFront(Room room, RoomTile tile, int rotation) {
    return room.getLayout().getTileInFront(tile, rotation);
  }

  public static int getDirectionOffset(int userRotation) {
    return Math.abs((userRotation + 4) % 8);
  }

  private static void giveEnable(Room room, RoomUnit habbo) {
    if (habbo.getEffectId() == 19) {
      return;
    }

    room.giveEffect(habbo, 19, -1);
  }

  public static boolean walkPrisoners(Habbo habbo, RoomTile toLocation, RoomTile fromLocation) {
    RoomUnit unit = habbo.getRoomUnit();
    List<Integer> escorted = RolePlay.getEscortManager().getEscorted(habbo.getHabboInfo().getId());
    if (escorted == null || escorted.isEmpty()) {
      return false;
    }
    unit.setFastWalk(true);
    unit.getRoom().getHabbos().stream().filter(h -> onEscortList(h, escorted)).forEach(
        prisoner -> EscortManager.walkPrisoner(prisoner, toLocation, fromLocation,
            unit.getBodyRotation().getValue()));
    return true;
  }

  public static boolean onEscortList(Habbo prisoner, List<Integer> users) {
    if (prisoner == null || prisoner.getHabboInfo() == null) {
      return false;
    }

    return users.contains(prisoner.getHabboInfo().getId());
  }
}
