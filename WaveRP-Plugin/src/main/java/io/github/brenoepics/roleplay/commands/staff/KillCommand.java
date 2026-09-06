package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.LiveFeed;

public class KillCommand extends Command {

  public KillCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();
    if (params.length != 2) {
      staff.whisper(":kill <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(params[1]);
    if (target == null) {
      staff.whisper("Ce joueur est introuvable ou hors ligne.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (target == staff) {
      staff.whisper("Vous ne pouvez pas vous tuer vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (avatar == null) {
      staff.whisper("Les donn\u00e9es RP de ce joueur sont indisponibles.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (avatar.isDead()) {
      staff.whisper("Ce joueur est d\u00e9j\u00e0 inconscient.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    avatar.kill();
    String staffName = staff.getHabboInfo().getUsername();
    String targetName = target.getHabboInfo().getUsername();

    if (avatar.isDead()) {
      LiveFeed.sendGlobalAlert(LiveFeed.alert("[DECES] " + targetName + " est mort."));
    }

    staff.whisper("Vous avez tu\u00e9 " + targetName + ".", RoomChatMessageBubbles.NORMAL);
    target.whisper("Vous avez \u00e9t\u00e9 tu\u00e9 par le staff " + staffName + ".",
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
