package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.hospital.ems.EmsService.CallResult;

public class EmsCloseCommand extends Command {

  public EmsCloseCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (params.length < 2) {
      gameClient.getHabbo().whisper("Utilisation : :fermerems <numero>",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    try {
      CallResult result = RolePlay.getEmsService().closeCall(gameClient.getHabbo(),
          Long.parseLong(params[1]));
      gameClient.getHabbo().whisper(result.message(), result.success()
          ? RoomChatMessageBubbles.RADIO : RoomChatMessageBubbles.ALERT);
    } catch (NumberFormatException exception) {
      gameClient.getHabbo().whisper("Le numero d'appel est invalide.",
          RoomChatMessageBubbles.ALERT);
    }
    return true;
  }
}

