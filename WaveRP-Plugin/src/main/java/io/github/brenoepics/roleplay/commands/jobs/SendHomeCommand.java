package io.github.brenoepics.roleplay.commands.jobs;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.DEFAULT_SEND_HOME_TIME;
import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.SEND_HOME_MAX;
import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.SEND_HOME_MIN;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class SendHomeCommand extends Command {

  public SendHomeCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data.getJobEntity() == null || data.getJobEntity()
        .equals(RolePlay.getJobService().getUnemployedJob()) || !data.getJobRankEntity().isManager()
        || !data.isDuty()) {
      return true;
    }

    if (params.length < 2) {
      habbo.whisper(":renvoyer <pseudo> <minutes>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = habbo.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (target == null) {
      habbo.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData == null) {
      habbo.whisper("Les donn\u00e9es RP de " + params[1] + " sont introuvables.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (target == habbo) {
      habbo.whisper("Vous ne pouvez pas vous renvoyer vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetData.getJobEntity() == null || targetData.getJobEntity()
        .equals(RolePlay.getJobService().getUnemployedJob())) {
      habbo.whisper("Le joueur " + params[1] + " n'a aucun m\u00e9tier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetData.getJobEntity().getId() != data.getJobEntity().getId()) {
      habbo.whisper("Le joueur " + params[1] + " n'exerce pas le m\u00eame m\u00e9tier que vous.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetData.getJobRankEntity().isHigherOrEqualThan(data.getJobRankEntity())) {
      habbo.whisper("Vous ne pouvez pas renvoyer une personne de grade \u00e9gal ou sup\u00e9rieur.",
          RoomChatMessageBubbles.ALERT);
    }

    int minutes = DEFAULT_SEND_HOME_TIME;

    if (params.length == 3) {
      try {
        minutes = Integer.parseInt(params[2]);
      } catch (Exception e) {
        habbo.whisper("La dur\u00e9e est invalide.", RoomChatMessageBubbles.ALERT);
        return true;
      }
    }

    if (minutes < SEND_HOME_MIN || minutes > SEND_HOME_MAX) {
      habbo.whisper("La dur\u00e9e doit \u00eatre comprise entre " + SEND_HOME_MIN + " et " + SEND_HOME_MAX
          + " minutes.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getJobsManager().getWorkCountDown()
        .getTimeOut(habbo.getHabboInfo().getId());
    if (timeout != null) {
      habbo.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
      return true;
    }

    if (!data.isDuty()) {
      gameClient.getHabbo().whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getJobsManager().sendHome(targetData, target, habbo, minutes);
    return true;
  }
}
