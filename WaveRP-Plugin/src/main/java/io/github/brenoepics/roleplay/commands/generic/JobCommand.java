package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Optional;

public class JobCommand extends Command {

  public JobCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (params.length != 4) {
      gameClient.getHabbo()
          .whisper(":job <user> <jobname (police/hospital/starbucks/armory)> <rank>",
              RoomChatMessageBubbles.ALERT);
      return true;
    }
    Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (habbo == null) {
      gameClient.getHabbo()
          .whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    Optional<JobEntity> job = RolePlay.getJobService().getJobByName(params[2]);
    if (job.isEmpty()) {
      gameClient.getHabbo()
          .whisper("Job " + params[2] + " not found!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String jobString = params[3];
    JobRankEntity jobRank = RolePlay.getJobService().getRankByJobAndName(job.get(), jobString)
        .orElse(null);

    if (jobRank == null) {
      try {
        int jobRankLevel = Integer.parseInt(jobString);
        jobRank = RolePlay.getJobService().getRankByJobAndLevel(job.get(), jobRankLevel)
            .orElse(null);
      } catch (NumberFormatException e) {
        gameClient.getHabbo().whisper("Rank level must be a number!", RoomChatMessageBubbles.ALERT);
        return true;
      }
    }

    if (jobRank == null) {
      gameClient.getHabbo()
          .whisper("Rank " + jobString + " not found!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    data.setJobEntity(job.orElse(null));
    data.setJobRankEntity(jobRank);
    data.updateDatabase();
    gameClient.getHabbo().whisper(
        "You have set the job of " + habbo.getHabboInfo().getUsername() + " to " + job.get()
            .getDisplayName() + " with rank " + jobRank.getDisplayName(),
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
