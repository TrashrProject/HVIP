package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class SuperHealCommand extends Command {

  public SuperHealCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) throws Exception {
    Habbo healer = gameClient.getHabbo();

    if (strings.length <= 1) {
      healer.whisper(":superheal <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = strings[1];
    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);

    if (target == null) {
      healer.whisper("Le joueur " + username + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (avatar != null) {
      avatar.heal();
      avatar.resetHungry();
      target.whisper("Un membre du staff vous a enti\u00e8rement soign\u00e9.", RoomChatMessageBubbles.NORMAL);
      healer.whisper("Vous avez soign\u00e9 " + username + ".", RoomChatMessageBubbles.NORMAL);
    } else {
      healer.whisper("Impossible de soigner " + username + ".", RoomChatMessageBubbles.ALERT);
    }

    return true;
  }
}
