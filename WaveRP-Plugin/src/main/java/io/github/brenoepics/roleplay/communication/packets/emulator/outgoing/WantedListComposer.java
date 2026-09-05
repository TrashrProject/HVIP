package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import java.sql.Timestamp;
import java.time.format.DateTimeFormatter;
import java.util.List;

public class WantedListComposer extends MessageComposer {

  private final List<WantedEntry> wantedEntries;
  private final DateTimeFormatter dateTimeFormatter;

  public WantedListComposer(List<WantedEntry> wantedEntries,
      DateTimeFormatter dateTimeFormatter) {
    this.wantedEntries = wantedEntries;
    this.dateTimeFormatter = dateTimeFormatter;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6003);
    this.response.appendInt(this.wantedEntries.size());

    for (WantedEntry entry : this.wantedEntries) {
      this.response.appendString(entry.username);
      this.response.appendString(entry.look == null ? "" : entry.look);
      this.response.appendInt(Math.min(entry.stars, 5));
      this.response.appendString(entry.endTime == null ? ""
          : entry.endTime.toLocalDateTime().format(dateTimeFormatter));
    }

    return this.response;
  }

  public static class WantedEntry {
    private final int userId;
    private final String username;
    private final String look;
    private int stars;
    private Timestamp endTime;

    public WantedEntry(int userId, String username, String look) {
      this.userId = userId;
      this.username = username;
      this.look = look;
    }

    public int getUserId() {
      return userId;
    }

    public void addCrime(int crimeStars, Timestamp crimeEndTime) {
      this.stars = Math.min(5, this.stars + Math.max(0, crimeStars));
      if (crimeEndTime != null && (this.endTime == null || crimeEndTime.after(this.endTime))) {
        this.endTime = crimeEndTime;
      }
    }
  }
}
