package io.github.brenoepics.roleplay.commands.jobs;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class QuitJobCommand extends Command {

  public QuitJobCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);

    if (data.getJobEntity() == null || data.getJobEntity().equals(RolePlay.getJobService().getUnemployedJob())) {
      habbo.whisper("You are currently unemployeed!", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (params.length != 1) {
      habbo.whisper(":quitjob", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getJobsManager().quitJob(habbo, data);
    return true;
  }

}
