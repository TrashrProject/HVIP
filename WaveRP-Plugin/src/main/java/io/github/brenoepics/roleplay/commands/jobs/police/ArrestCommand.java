package io.github.brenoepics.roleplay.commands.jobs.police;

import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class ArrestCommand extends Command {

  public ArrestCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo manager = gameClient.getHabbo();
    RpAvatar managerData = RolePlay.getAvatarManager().getRpAvatar(manager);

    if (!managerData.getJobRankEntity().hasPermission(JobPermissions.POLICE_ARREST)) {
      manager.whisper("Vous n'\u00eates pas policier.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!managerData.isDuty()) {
      manager.whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!managerData.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length < 2) {
      manager.whisper(":arreterpolice <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("arrest")
        .getTimeOut(manager.getHabboInfo().getId());
    if (timeout != null) {
      manager.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
      return true;
    }

    Habbo habbo = manager.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (habbo == null) {
      manager.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (habbo == manager) {
      manager.whisper("Vous ne pouvez pas vous arr\u00eater vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int distanceX = habbo.getRoomUnit().getX() - manager.getRoomUnit().getX();
    int distanceY = habbo.getRoomUnit().getY() - manager.getRoomUnit().getY();

    if (distanceX < -1 || distanceX > 1 || distanceY < -1 || distanceY > 1) {
      manager.whisper(Emulator.getTexts().getValue("commands.error.cmd_pull.cant_reach")
          .replace("%user%", params[1]), RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (RolePlay.getWantedManager().getUserWantedStars(habbo.getHabboInfo().getId()) <= 0) {
      manager.whisper("Ce joueur n'est pas recherch\u00e9.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getWantedManager().arrestUser(manager, habbo);
    managerData.executeAction();
    return true;
  }
}
