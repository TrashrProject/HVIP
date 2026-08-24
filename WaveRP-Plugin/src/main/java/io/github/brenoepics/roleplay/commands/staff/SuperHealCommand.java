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
      healer.whisper(":superheal <username>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = strings[1];
    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);

    if (target == null) {
      healer.whisper("Player " + username + " not found.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (avatar != null) {
      avatar.heal();
      avatar.resetHungry();
      target.whisper("You have been super-healed by staff!", RoomChatMessageBubbles.NORMAL);
      healer.whisper("You have healed " + username + ".", RoomChatMessageBubbles.NORMAL);
    } else {
      healer.whisper("Could not heal " + username + ".", RoomChatMessageBubbles.ALERT);
    }

    return true;
  }
}