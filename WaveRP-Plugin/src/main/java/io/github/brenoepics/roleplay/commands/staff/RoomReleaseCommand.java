package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class RoomReleaseCommand extends Command {

  public RoomReleaseCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();
    Room room = staff.getHabboInfo().getCurrentRoom();

    if (room == null) {
      staff.whisper("Vous n'\u00eates dans aucune salle.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int releasedCount = 0;
    for (Habbo user : room.getHabbos()) {
      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(user);
      if (avatar != null && avatar.isJailed()) {
        RolePlay.getPrisonService().releaseFromJail(user, avatar);
        user.whisper("Un membre du staff vous a lib\u00e9r\u00e9 de prison.", RoomChatMessageBubbles.ALERT);
        releasedCount++;
      }
    }

    if (releasedCount == 0) {
      staff.whisper("Aucun prisonnier n'a \u00e9t\u00e9 trouv\u00e9 dans cette salle.", RoomChatMessageBubbles.ALERT);
    } else {
      staff.whisper(releasedCount + " prisonnier(s) lib\u00e9r\u00e9(s) dans cette salle.",
          RoomChatMessageBubbles.NORMAL);
    }

    return true;
  }
}
