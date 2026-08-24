package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import com.eu.habbo.habbohotel.users.HabboManager;
import org.jetbrains.annotations.Nullable;

public class BucksCommand extends Command {

  public static final String COMMANDS_ERROR_CMD_CREDITS_INVALID_AMOUNT = "commands.error.cmd_credits.invalid_amount";

  public BucksCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (params.length != 3) {
      gameClient.getHabbo()
          .whisper(Emulator.getTexts().getValue(COMMANDS_ERROR_CMD_CREDITS_INVALID_AMOUNT),
              RoomChatMessageBubbles.ALERT);
      return true;
    }

    HabboInfo info = HabboManager.getOfflineHabboInfo(params[1]);

    if (info == null) {
      gameClient.getHabbo().whisper(
          Emulator.getTexts().getValue("commands.error.cmd_credits.user_not_found")
              .replace("%amount%", Integer.parseInt(params[2]) + "").replace("%user%", params[1]),
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo habbo = Emulator.getGameServer().getGameClientManager().getHabbo(params[1]);

    Integer bucks = getBucks(gameClient, params);
    if (bucks == null) {
      return true;
    }

    if (habbo == null) {
      Emulator.getGameEnvironment().getHabboManager().giveCredits(info.getId(), bucks);
      gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.succes.cmd_credits.send")
              .replace("%amount%", Integer.parseInt(params[2]) + "").replace("%user%", params[1]),
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (bucks == 0) {
      gameClient.getHabbo()
          .whisper(Emulator.getTexts().getValue(COMMANDS_ERROR_CMD_CREDITS_INVALID_AMOUNT),
              RoomChatMessageBubbles.ALERT);
      return true;
    }

    habbo.givePoints(200, bucks);
    if (habbo.getHabboInfo().getCurrentRoom() != null) {
      habbo.whisper(Emulator.getTexts().getValue("commands.generic.cmd_credits.received")
          .replace("%amount%", Integer.parseInt(params[2]) + ""), RoomChatMessageBubbles.ALERT);
    } else {
      habbo.alert(Emulator.getTexts().getValue("commands.generic.cmd_credits.received")
          .replace("%amount%", Integer.parseInt(params[2]) + ""));
    }

    gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.succes.cmd_credits.send")
            .replace("%amount%", Integer.parseInt(params[2]) + "").replace("%user%", params[1]),
        RoomChatMessageBubbles.ALERT);

    return true;
  }

  private static @Nullable Integer getBucks(GameClient gameClient, String[] params) {
    int bucks;
    try {
      bucks = Integer.parseInt(params[2]);
    } catch (NumberFormatException e) {
      gameClient.getHabbo()
          .whisper(Emulator.getTexts().getValue(COMMANDS_ERROR_CMD_CREDITS_INVALID_AMOUNT),
              RoomChatMessageBubbles.ALERT);
      return null;
    }
    return bucks;
  }
}
