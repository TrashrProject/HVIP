package io.github.brenoepics.roleplay.commands.wanted;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import com.eu.habbo.habbohotel.users.HabboManager;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.WantedListComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.WantedListComposer.WantedEntry;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

public class WantedListCommand extends Command {

  public static final DateTimeFormatter DATE_TIME_FORMATTER = DateTimeFormatter.ofPattern(
      "dd/MM/yyyy HH:mm:ss");

  public WantedListCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) {
    if (strings.length > 0 && "unwanted".equalsIgnoreCase(strings[0])) {
      return handleUnwanted(gameClient, strings);
    }

    List<WantedEntry> wanted = loadWantedEntries();

    if (wanted.isEmpty()) {
      gameClient.getHabbo().whisper("Aucun joueur n'est actuellement recherché.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    gameClient.sendResponse(new WantedListComposer(wanted, DATE_TIME_FORMATTER));
    return true;
  }

  private boolean handleUnwanted(GameClient gameClient, String[] strings) {
    Habbo officer = gameClient.getHabbo();
    RpAvatar officerData = RolePlay.getAvatarManager().getRpAvatar(officer);

    if (officerData == null || officerData.getJobRankEntity() == null
        || !officerData.getJobRankEntity().hasPermission(JobPermissions.POLICE_ARREST)) {
      officer.whisper("Vous n'êtes pas policier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.isDuty()) {
      officer.whisper("Vous devez être en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (strings.length < 2) {
      officer.whisper("Usage : :unwanted <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo onlineTarget = Emulator.getGameEnvironment().getHabboManager().getHabbo(strings[1]);
    HabboInfo targetInfo = onlineTarget != null
        ? onlineTarget.getHabboInfo()
        : HabboManager.getOfflineHabboInfo(strings[1]);

    if (targetInfo == null) {
      officer.whisper("Le joueur " + strings[1] + " est introuvable.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetInfo.getId() == officer.getHabboInfo().getId()) {
      officer.whisper("Vous ne pouvez pas vous retirer vous-même de la liste des recherchés.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    int updated = clearWantedRecords(targetInfo.getId());
    if (updated <= 0) {
      officer.whisper(targetInfo.getUsername() + " n'est pas actuellement recherché.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getWantedManager().getCachedWantedList().remove(targetInfo.getId());

    if (officer.getHabboInfo().getCurrentRoom() != null) {
      officer.shout("* Retire " + targetInfo.getUsername() + " de la liste des recherchés *",
          RoomChatMessageBubbles.AMBASSADOR);
    }

    officer.whisper(targetInfo.getUsername() + " a été retiré de la liste des recherchés.",
        RoomChatMessageBubbles.ALERT);
    if (onlineTarget != null) {
      onlineTarget.whisper("Vous n'êtes plus recherché par la Police Nationale.",
          RoomChatMessageBubbles.ALERT);
    }

    return true;
  }

  private static int clearWantedRecords(int userId) {
    String sql = "UPDATE user_criminal_records "
        + "SET served_time = 1 "
        + "WHERE user_id = ? AND served_time = 0 AND paid_fine = 0 "
        + "AND ends_at IS NOT NULL AND ends_at > CURRENT_TIMESTAMP";

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
         PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, userId);
      return statement.executeUpdate();
    } catch (SQLException exception) {
      Emulator.getLogging().logSQLException(exception);
      return 0;
    }
  }

  private static List<WantedEntry> loadWantedEntries() {
    Map<Integer, WantedEntry> entries = new LinkedHashMap<>();
    String sql = "SELECT r.user_id, u.username, u.look, c.stars, r.ends_at "
        + "FROM user_criminal_records r "
        + "INNER JOIN users u ON u.id = r.user_id "
        + "INNER JOIN crimes c ON c.id = r.crime_id "
        + "WHERE r.served_time = 0 AND r.paid_fine = 0 "
        + "AND r.ends_at IS NOT NULL AND r.ends_at > CURRENT_TIMESTAMP "
        + "ORDER BY u.username ASC, r.charged_at DESC";

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
         PreparedStatement statement = connection.prepareStatement(sql);
         ResultSet set = statement.executeQuery()) {
      while (set.next()) {
        int userId = set.getInt("user_id");
        WantedEntry entry = entries.get(userId);
        if (entry == null) {
          entry = new WantedEntry(userId, set.getString("username"), set.getString("look"));
          entries.put(userId, entry);
        }
        entry.addCrime(set.getInt("stars"), set.getTimestamp("ends_at"));
      }
    } catch (SQLException exception) {
      Emulator.getLogging().logSQLException(exception);
    }

    return new ArrayList<>(entries.values());
  }
}
