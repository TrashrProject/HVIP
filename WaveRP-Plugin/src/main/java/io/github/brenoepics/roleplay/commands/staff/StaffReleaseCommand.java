package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class StaffReleaseCommand extends Command {

  public StaffReleaseCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();

    if (params.length < 2) {
      staff.whisper(":libererstaff <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = params[1];
    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);

    if (target == null) {
      staff.whisper("Le joueur " + username + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (!targetData.isJailed()) {
      staff.whisper("Ce joueur n'est pas emprisonn\u00e9.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getPrisonService().releaseFromJail(target, targetData);
    staff.whisper("Vous avez lib\u00e9r\u00e9 " + username + " de prison.", RoomChatMessageBubbles.NORMAL);
    target.whisper(
        "Vous avez \u00e9t\u00e9 lib\u00e9r\u00e9 de prison par " + staff.getHabboInfo().getUsername() + ".",
        RoomChatMessageBubbles.ALERT);

    return true;
  }
}
