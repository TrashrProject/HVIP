package io.github.brenoepics.roleplay.commands.jobs;

import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.template.PassiveTemplates;

public class HireCommand extends Command {

  public HireCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo manager = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(manager);

    if (data.getJobEntity() == null || data.getJobEntity()
        .equals(RolePlay.getJobService().getUnemployedJob())) {
      manager.whisper("Vous n'avez aucun m\u00e9tier.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (params.length != 2) {
      manager.whisper(":recruter <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo habbo = manager.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (habbo == null) {
      manager.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!data.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas \u00e0 recruter.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);

    if (!targetData.getJobEntity().equals(RolePlay.getJobService().getUnemployedJob())) {
      manager.whisper("Vous ne pouvez pas recruter " + habbo.getHabboInfo().getUsername() + ".",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    JobRankEntity job = data.getJobEntity().getRanks().getFirst();
    if (job == null || job.equals(RolePlay.getJobService().getUnemployedRank())) {
      manager.whisper("Vous ne pouvez pas recruter " + habbo.getHabboInfo().getUsername() + ".",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.isDuty()) {
      gameClient.getHabbo().whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    targetData.setJobEntity(data.getJobEntity());
    targetData.setJobRankEntity(job);
    manager.whisper("Vous avez recrut\u00e9 " + habbo.getHabboInfo().getUsername() + ".", RoomChatMessageBubbles.NORMAL);
    habbo.whisper("Vous avez \u00e9t\u00e9 recrut\u00e9 par " + manager.getHabboInfo().getUsername() + ".",
        RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    String employeeName = habbo.getHabboInfo().getUsername();
    data.executeAction();
    LiveFeed.sendGlobalAlert(
        LiveFeed.alert(PassiveTemplates.HIRE.format(managerName, employeeName, job.getDisplayName())));
    return true;
  }
}
