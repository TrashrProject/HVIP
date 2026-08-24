package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.hospital.ems.EmsService.CallResult;
import java.util.Arrays;

public class EmsCallCommand extends Command {

  public EmsCallCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    String reason = params.length > 1
        ? String.join(" ", Arrays.copyOfRange(params, 1, params.length))
        : "Urgence medicale";
    CallResult result = RolePlay.getEmsService().createCall(habbo, reason);
    habbo.whisper(result.message(), result.success()
        ? RoomChatMessageBubbles.RADIO : RoomChatMessageBubbles.ALERT);
    return true;
  }
}

