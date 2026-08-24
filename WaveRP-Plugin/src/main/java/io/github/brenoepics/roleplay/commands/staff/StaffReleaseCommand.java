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
      staff.whisper(":staffrelease <username>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = params[1];
    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);

    if (target == null) {
      staff.whisper("Player " + username + " not found.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (!targetData.isJailed()) {
      staff.whisper("This user is not jailed!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getPrisonService().releaseFromJail(target, targetData);
    staff.whisper("You have released " + username + " from jail.", RoomChatMessageBubbles.NORMAL);
    target.whisper(
        "You have been released from jail by the staff " + staff.getHabboInfo().getUsername(),
        RoomChatMessageBubbles.ALERT);

    return true;
  }
}