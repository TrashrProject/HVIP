package io.github.brenoepics.roleplay.commands.wanted;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.WantedListComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.WantedListComposer.WantedEntry;
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
    List<WantedEntry> wanted = loadWantedEntries();

    if (wanted.isEmpty()) {
      gameClient.getHabbo().whisper("Aucun joueur n'est actuellement recherché.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    gameClient.sendResponse(new WantedListComposer(wanted, DATE_TIME_FORMATTER));
    return true;
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
