package io.github.brenoepics.roleplay.commands.wanted;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.WantedListComposer;
import io.github.brenoepics.roleplay.features.crime.wantedlist.CriminalRecord;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.Map;

public class WantedListCommand extends Command {

  public static final DateTimeFormatter DATE_TIME_FORMATTER = DateTimeFormatter.ofPattern(
      "yyyy-MM-dd HH:mm:ss");

  public WantedListCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) throws Exception {
    Map<Habbo, List<CriminalRecord>> onlineCrimes = RolePlay.getWantedManager()
        .getOnlineUsersCrimes();

    if (onlineCrimes.isEmpty()) {
      gameClient.getHabbo().whisper("There are no users on the wanted list.",
          com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles.ALERT);
      return true;
    }

    gameClient.sendResponse(new WantedListComposer(onlineCrimes, DATE_TIME_FORMATTER));
    return true;
  }
}
