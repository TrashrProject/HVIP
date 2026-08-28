package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.features.crime.wantedlist.CriminalRecord;
import java.sql.Timestamp;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.Map;
import java.util.Optional;

public class WantedListComposer extends MessageComposer {

  private final Map<Habbo, List<CriminalRecord>> onlineCrimes;
  private final DateTimeFormatter dateTimeFormatter;

  public WantedListComposer(Map<Habbo, List<CriminalRecord>> onlineCrimes,
      DateTimeFormatter dateTimeFormatter) {
    this.onlineCrimes = onlineCrimes;
    this.dateTimeFormatter = dateTimeFormatter;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6003);

    this.response.appendInt(this.onlineCrimes.size());

    for (Map.Entry<Habbo, List<CriminalRecord>> entry : this.onlineCrimes.entrySet()) {
      String username = entry.getKey().getHabboInfo().getUsername();
      Optional<Timestamp> end = io.github.brenoepics.roleplay.RolePlay.getWantedManager()
          .getUserWantedEndTimestamp(entry.getValue());
      int stars = io.github.brenoepics.roleplay.RolePlay.getWantedManager()
          .getUserWantedStars(entry.getValue());

      this.response.appendString(username);
      this.response.appendString(entry.getKey().getHabboInfo().getLook());
      this.response.appendInt(stars);

      if (end.isPresent()) {
        String readableDateTime = end.get().toLocalDateTime().format(dateTimeFormatter);
        this.response.appendString(readableDateTime);
      } else {
        this.response.appendString(""); // or some default value
      }
    }

    return this.response;
  }
}