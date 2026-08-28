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
      staff.whisper(":statistiques <pseudo> <sante|energie|faim|protection> <valeur>",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = params[1];
    String stat = switch (params[2].toLowerCase()) {
      case "sante" -> "health";
      case "energie" -> "energy";
      case "faim" -> "hunger";
      case "protection" -> "shield";
      default -> params[2].toLowerCase();
    };
    String amountStr = params[3];

    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);
    if (target == null) {
      staff.whisper("Le joueur " + username + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (avatar == null) {
      staff.whisper("Les donn\u00e9es RP de " + username + " sont introuvables.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    int amount;
    try {
      amount = Integer.parseInt(amountStr);
    } catch (NumberFormatException e) {
      staff.whisper("La valeur doit \u00eatre un nombre.", RoomChatMessageBubbles.ALERT);
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
        staff.whisper("Statistique invalide : sante, energie, faim ou protection.",
            RoomChatMessageBubbles.ALERT);
        return true;
    }

    avatar.updateDatabase();
    String displayedStat = switch (stat) {
      case "health" -> "sant\u00e9";
      case "energy" -> "\u00e9nergie";
      case "hunger" -> "faim";
      case "shield" -> "protection";
      default -> stat;
    };
    staff.whisper("La statistique " + displayedStat + " de " + username + " vaut maintenant " + amount + ".",
        RoomChatMessageBubbles.NORMAL);
    target.whisper("Le staff a d\u00e9fini votre statistique " + displayedStat + " sur " + amount + ".",
        RoomChatMessageBubbles.ALERT);

    return true;
  }
}
