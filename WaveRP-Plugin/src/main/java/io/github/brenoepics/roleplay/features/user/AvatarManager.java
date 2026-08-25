package io.github.brenoepics.roleplay.features.user;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.permissions.Permission;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomManager;
import com.eu.habbo.habbohotel.rooms.RoomState;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.ForwardToRoomComposer;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.HashMap;
import java.util.Map;
import lombok.Getter;
import lombok.Setter;
import lombok.extern.slf4j.Slf4j;
import org.jetbrains.annotations.Nullable;

@Getter
@Slf4j
@Setter
public class AvatarManager {

  private static final String SPAWN_FORWARD_CACHE_KEY = "roleplay.spawn_forwarded";

  private Map<Habbo, RpAvatar> cachedData = new HashMap<>();

  public RpAvatar getRpAvatar(Habbo habbo) {
    RpAvatar cached = cachedData.get(habbo);
    if (cached != null) {
      return cached;
    }

    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "SELECT * FROM `users_roleplay` WHERE user_id = ?")) {
      statement.setInt(1, habbo.getHabboInfo().getId());
      try (ResultSet set = statement.executeQuery()) {
        if (!set.next()) {
          log.info("No Roleplay Data found for {} creating data now..",
              habbo.getHabboInfo().getUsername());
          RpAvatar user = RpAvatar.createUser(habbo, connection);
          cachedData.put(habbo, user);
          return user;
        }
        RpAvatar user = RpAvatar.loadUser(habbo, set, connection);
        cachedData.put(habbo, user);
        return user;
      }
    } catch (SQLException e) {
      log.error("Error while loading RolePlay data for {}", habbo.getHabboInfo().getUsername(), e);
      return null;
    }
  }

  public static void sendToSpawn(@Nullable Habbo habbo, RpAvatar data) {
    if (habbo == null || habbo.getClient() == null || data == null
        || Boolean.TRUE.equals(habbo.getHabboStats().cache.get(SPAWN_FORWARD_CACHE_KEY))) {
      return;
    }
    GameClient client = habbo.getClient();

    if (data.getJailTime() > Emulator.getIntUnixTimestamp()) {
      data.setJailed(true);
      int jailRoomId = Emulator.getConfig().getInt("nahabbo.features.jail.roomid");
      if (Emulator.getGameEnvironment().getRoomManager().loadRoom(jailRoomId) != null) {
        forwardOnce(habbo, client, jailRoomId);
        return;
      }
    }

    AvatarLocation lastPosition = data.getLastPosition();
    if (lastPosition != null) {
      Room lastRoom = Emulator.getGameEnvironment().getRoomManager()
          .loadRoom(lastPosition.getRoomId());
      if (canRestoreRoom(habbo, lastRoom)) {
        forwardOnce(habbo, client, lastRoom.getId());
        return;
      }
    }

    int fallbackRoomId = getFallbackRoomId(habbo);
    if (fallbackRoomId <= 0) {
      log.warn("No valid reconnect room found for {}", habbo.getHabboInfo().getUsername());
      return;
    }

    forwardOnce(habbo, client, fallbackRoomId);
  }

  public static void resetSpawnForward(@Nullable Habbo habbo) {
    if (habbo != null) {
      habbo.getHabboStats().cache.remove(SPAWN_FORWARD_CACHE_KEY);
    }
  }

  public static void retrySpawnIfRoomless(@Nullable Habbo habbo, RpAvatar data) {
    if (habbo == null || habbo.getHabboInfo().getCurrentRoom() != null) {
      return;
    }

    resetSpawnForward(habbo);
    sendToSpawn(habbo, data);
  }

  private static void forwardOnce(Habbo habbo, GameClient client, int roomId) {
    habbo.getHabboStats().cache.put(SPAWN_FORWARD_CACHE_KEY, true);
    log.info("Forwarding {} to reconnect room {}", habbo.getHabboInfo().getUsername(), roomId);
    client.sendResponse(new ForwardToRoomComposer(roomId));
  }

  private static int getFallbackRoomId(Habbo habbo) {
    int personalHome = habbo.getHabboInfo().getHomeRoom();
    Room personalHomeRoom = Emulator.getGameEnvironment().getRoomManager().loadRoom(personalHome);
    if (canRestoreRoom(habbo, personalHomeRoom)) {
      return personalHome;
    }

    Room globalHome = Emulator.getGameEnvironment().getRoomManager()
        .loadRoom(RoomManager.HOME_ROOM_ID);
    return canRestoreRoom(habbo, globalHome) ? globalHome.getId() : 0;
  }

  private static boolean canRestoreRoom(Habbo habbo, Room room) {
    if (room == null || room.isBanned(habbo)) {
      return false;
    }

    return room.getState() == RoomState.OPEN || room.isOwner(habbo) || room.hasRights(habbo)
        || habbo.hasPermission(Permission.ACC_ANYROOMOWNER)
        || habbo.hasPermission(Permission.ACC_ENTERANYROOM);
  }
}
