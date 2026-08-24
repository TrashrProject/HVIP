package io.github.brenoepics.roleplay.features.user;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomManager;
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
    if (habbo == null || habbo.getClient() == null) {
      return;
    }
    GameClient client = habbo.getClient();

    if (data.getJailTime() > Emulator.getIntUnixTimestamp()) {
      data.setJailed(true);
      client.sendResponse(
          new ForwardToRoomComposer(Emulator.getConfig().getInt("nahabbo.features.jail.roomid")));
      return;
    }

    if (data.getLastPosition() == null) {
      client.sendResponse(new ForwardToRoomComposer(RoomManager.HOME_ROOM_ID));
      return;
    }

    client.sendResponse(new ForwardToRoomComposer(data.getLastPosition().getRoomId()));
  }
}
