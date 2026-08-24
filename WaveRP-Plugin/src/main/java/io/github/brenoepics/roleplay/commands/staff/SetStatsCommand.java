package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class SetStatsCommand extends Command {

  public SetStatsCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();

    if (params.length < 4) {
      staff.whisper(":setstats <username> <health|energy|hunger|shield> <amount>",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = params[1];
    String stat = params[2].toLowerCase();
    String amountStr = params[3];

    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);
    if (target == null) {
      staff.whisper("Player " + username + " not found.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (avatar == null) {
      staff.whisper("Could not find avatar data for " + username + ".",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    int amount;
    try {
      amount = Integer.parseInt(amountStr);
    } catch (NumberFormatException e) {
      staff.whisper("Amount must be a number.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    switch (stat) {
      case "health":
        avatar.setHealth(amount);
        avatar.setDead(amount <= 0);
        avatar.updateLife();
        break;
      case "energy":
        avatar.setEnergy(amount);
        avatar.updateLife();
        break;
      case "hunger":
        avatar.setHunger(amount);
        avatar.updateLife();
        break;
      case "shield":
        avatar.setShield(amount);
        avatar.updateLife();
        break;
      default:
        staff.whisper("Invalid stat. Valid options: health, energy, hunger, shield.",
            RoomChatMessageBubbles.ALERT);
        return true;
    }

    avatar.updateDatabase();
    staff.whisper("Set " + stat + " of " + username + " to " + amount + ".",
        RoomChatMessageBubbles.NORMAL);
    target.whisper("Your " + stat + " has been set to " + amount + " by staff.",
        RoomChatMessageBubbles.ALERT);

    return true;
  }
}