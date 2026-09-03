package io.github.brenoepics.roleplay.commands.jobs;

import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class StartWorkCommand extends Command {

  public StartWorkCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);

    if (data.getJobEntity() == null || data.getJobEntity().equals(RolePlay.getJobService().getUnemployedJob())) {
      habbo.whisper("Vous n'avez aucun metier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 1) {
      habbo.whisper(":travailler", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (data.isDead()) {
      habbo.whisper("Vous ne pouvez pas travailler tant que vous etes inconscient.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (habbo.getHabboInfo().getCurrentRoom() == null || !RolePlay.getJobsManager()
        .canWorkAtRoom(data.getJobEntity(), habbo.getHabboInfo().getCurrentRoom().getId())) {
      habbo.whisper("Vous devez \u00eatre dans votre lieu de travail pour commencer votre service.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (data.isDuty()) {
      habbo.whisper("Vous travaillez deja.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getJobsManager().getWorkCountDown()
        .getTimeOut(habbo.getHabboInfo().getId());
    if (timeout != null) {
      habbo.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de reutiliser cette commande.");
      return true;
    }

    RolePlay.getJobsManager().startWork(gameClient, data, habbo);
    data.executeAction();
    return true;
  }

}
