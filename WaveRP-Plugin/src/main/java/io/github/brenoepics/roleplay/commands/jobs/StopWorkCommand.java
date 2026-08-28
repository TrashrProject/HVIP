package io.github.brenoepics.roleplay.commands.jobs;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class StopWorkCommand extends Command {

  public StopWorkCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);

    if (data.getJobEntity() == null || data.getJobEntity()
        .equals(RolePlay.getJobService().getUnemployedJob())) {
      habbo.whisper("Vous n'avez aucun metier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!RolePlay.getJobsManager()
        .canWorkAtRoom(data.getJobEntity(), habbo.getHabboInfo().getCurrentRoom().getId())) {
      habbo.whisper("Vous devez etre dans la salle de votre metier pour arreter de travailler.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 1) {
      habbo.whisper(":arreter", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.isDuty()) {
      habbo.whisper("Vous ne travaillez pas actuellement.", RoomChatMessageBubbles.ALERT);
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

    RolePlay.getJobsManager().stopWork(habbo, data);
    return true;
  }
}
