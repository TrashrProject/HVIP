package io.github.brenoepics.roleplay.commands.staff;

import static io.github.brenoepics.roleplay.features.crime.wantedlist.WantedSystemManager.executeArrest;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.time.Duration;

public class StaffArrestCommand extends Command {

  public StaffArrestCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) throws Exception {
    if (params.length < 3) {
      gameClient.getHabbo().whisper(":emprisonnerstaff <pseudo> <minutes>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo policeStaff = gameClient.getHabbo();
    Habbo criminal = Emulator.getGameEnvironment().getHabboManager().getHabbo(params[1]);
    if (criminal == null) {
      policeStaff.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (criminal == policeStaff) {
      policeStaff.whisper("Vous ne pouvez pas vous emprisonner vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar criminalData = RolePlay.getAvatarManager().getRpAvatar(criminal);

    int minutes = getMinutes(params);

    if (minutes <= 0) {
      policeStaff.whisper("Indiquez un nombre de minutes valide.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Duration duration = Duration.ofMinutes(minutes);

    executeArrest(policeStaff, criminal, "Intervention du staff", criminalData, duration);
    return true;
  }

  private static int getMinutes(String[] params) {
    try {
      return Integer.parseInt(params[2]);
    } catch (NumberFormatException e) {
      return 0;
    }
  }
}
