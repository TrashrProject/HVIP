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
          .whisper(":job <joueur> <m\u00e9tier (police/EMS/starbucks/armory)> <rank>",
              RoomChatMessageBubbles.ALERT);
      return true;
    }
    Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (habbo == null) {
      gameClient.getHabbo()
          .whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    String requestedJob = "ems".equalsIgnoreCase(params[2]) ? "hospital" : params[2];
    Optional<JobEntity> job = RolePlay.getJobService().getJobByName(requestedJob);
    if (job.isEmpty()) {
      gameClient.getHabbo()
          .whisper("Le m\u00e9tier " + params[2] + " est introuvable.", RoomChatMessageBubbles.ALERT);
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
        gameClient.getHabbo()
            .whisper("Le niveau du grade doit \u00eatre un nombre.", RoomChatMessageBubbles.ALERT);
        return true;
      }
    }

    if (jobRank == null) {
      gameClient.getHabbo()
          .whisper("Le grade " + jobString + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    data.setJobEntity(job.orElse(null));
    data.setJobRankEntity(jobRank);
    data.updateDatabase();
    gameClient.getHabbo().whisper(
        "Vous avez attribu\u00e9 le m\u00e9tier " + job.get().getDisplayName() + " au joueur "
            + habbo.getHabboInfo().getUsername() + " avec le grade " + jobRank.getDisplayName()
            + ".",
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
