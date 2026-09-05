package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomTileState;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.util.pathfinding.Rotation;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.crime.PoliceTaserService;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/** Session physique liee a un ordinateur et a la chaise placee devant celui-ci. */
public final class BankComputerSessionManager {
  private static final long SESSION_MS = 10 * 60 * 1000L;
  private static final Map<Integer, Session> SESSIONS = new ConcurrentHashMap<>();

  private record Session(int roomId, int itemId, short chairX, short chairY, long expiresAt) {}

  private BankComputerSessionManager() {}

  public static boolean isConfigured(int itemId) {
    return isConfigured(itemId, 0);
  }

  /**
   * Accepte les postes explicitement configures ainsi que tous les ordinateurs places dans une
   * salle rattachee au metier Banque. Un ordinateur ajoute plus tard dans la banque fonctionne
   * ainsi sans devoir enregistrer manuellement son identifiant d'instance.
   */
  public static boolean isConfigured(int itemId, int roomId) {
    String sql = "SELECT 1 FROM rp_bank_computer_items WHERE item_id=? AND active=1 "
        + "UNION ALL "
        + "SELECT 1 FROM jobs j JOIN jobs_rooms jr ON jr.job_id=j.id "
        + "WHERE ? > 0 AND j.active=1 AND LOWER(j.name)='bank' "
        + "AND FIND_IN_SET(CAST(? AS CHAR),REPLACE(jr.rooms,' ',''))>0 LIMIT 1";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, itemId);
      statement.setInt(2, roomId);
      statement.setInt(3, roomId);
      try (ResultSet result = statement.executeQuery()) {
        return result.next();
      }
    } catch (Exception exception) {
      return false;
    }
  }

  /** @return true quand la session vient d'etre ouverte, false quand elle a ete fermee. */
  public static boolean toggle(Habbo habbo, Room room, HabboItem computer) {
    int userId = habbo.getHabboInfo().getId();
    Session current = SESSIONS.get(userId);
    if (current != null && current.roomId == room.getId() && current.itemId == computer.getId()
        && current.expiresAt >= System.currentTimeMillis()) {
      disconnect(habbo);
      return false;
    }

    RoomTile chair = habbo.getRoomUnit().getCurrentLocation();
    SESSIONS.put(userId, new Session(room.getId(), computer.getId(), chair.x, chair.y,
        System.currentTimeMillis() + SESSION_MS));
    habbo.getRoomUnit().setCanWalk(false);
    habbo.getRoomUnit().cmdSit = true;
    habbo.getRoomUnit().statusUpdate(true);
    return true;
  }

  public static void disconnect(int userId) {
    SESSIONS.remove(userId);
  }

  public static void disconnect(Habbo habbo) {
    if (habbo == null || SESSIONS.remove(habbo.getHabboInfo().getId()) == null) return;
    int userId = habbo.getHabboInfo().getId();
    if (habbo.getRoomUnit() != null && !PoliceTaserService.isTased(userId)
        && !PoliceHandcuffService.isHandcuffed(userId)
        && !RolePlay.getEscortManager().isPrisonerEscorted(userId)) {
      habbo.getRoomUnit().setCanWalk(true);
      habbo.getRoomUnit().statusUpdate(true);
    }
  }

  public static boolean isAtAssignedChair(Habbo habbo, HabboItem computer) {
    if (habbo == null || computer == null || habbo.getRoomUnit() == null) return false;
    RoomTile chair = habbo.getRoomUnit().getCurrentLocation();
    if (chair == null || chair.state != RoomTileState.SIT) return false;
    if (Math.max(Math.abs(chair.x - computer.getX()), Math.abs(chair.y - computer.getY())) != 1) {
      return false;
    }
    int facing = Rotation.Calculate(chair.x, chair.y, computer.getX(), computer.getY());
    return habbo.getRoomUnit().getBodyRotation().getValue() == facing;
  }

  public static boolean isBankEmployeeOnDuty(Habbo habbo) {
    if (habbo == null) return false;
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    return avatar != null && avatar.isDuty() && avatar.getJobEntity() != null
        && "bank".equalsIgnoreCase(avatar.getJobEntity().getName());
  }

  public static boolean mayUsePersonalBankCommand(Habbo habbo) {
    return !isBankEmployeeOnDuty(habbo) || hasActiveSession(habbo);
  }

  public static boolean isUsingComputer(Habbo habbo) {
    return hasActiveSession(habbo);
  }

  public static boolean hasActiveSession(Habbo habbo) {
    if (habbo == null || habbo.getRoomUnit() == null) return false;
    int userId = habbo.getHabboInfo().getId();
    Session session = SESSIONS.get(userId);
    if (session == null) return false;

    Room room = habbo.getHabboInfo().getCurrentRoom();
    if (room == null || session.expiresAt < System.currentTimeMillis()
        || session.roomId != room.getId() || !isBankEmployeeOnDuty(habbo)) {
      disconnect(habbo);
      return false;
    }

    for (HabboItem item : room.getFloorItems()) {
      if (item.getId() != session.itemId) continue;
      RoomTile current = habbo.getRoomUnit().getCurrentLocation();
      boolean valid = current != null && current.x == session.chairX && current.y == session.chairY
          && isAtAssignedChair(habbo, item);
      if (!valid) disconnect(habbo);
      return valid;
    }

    disconnect(habbo);
    return false;
  }
}
