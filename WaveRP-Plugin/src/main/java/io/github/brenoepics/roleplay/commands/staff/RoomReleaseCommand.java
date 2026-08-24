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
      staff.whisper("You are not in a room.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int releasedCount = 0;
    for (Habbo user : room.getHabbos()) {
      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(user);
      if (avatar != null && avatar.isJailed()) {
        RolePlay.getPrisonService().releaseFromJail(user, avatar);
        user.whisper("You have been released from jail by staff.", RoomChatMessageBubbles.ALERT);
        releasedCount++;
      }
    }

    if (releasedCount == 0) {
      staff.whisper("No jailed users found in this room.", RoomChatMessageBubbles.ALERT);
    } else {
      staff.whisper("Released " + releasedCount + " jailed user(s) in this room.",
          RoomChatMessageBubbles.NORMAL);
    }

    return true;
  }
}