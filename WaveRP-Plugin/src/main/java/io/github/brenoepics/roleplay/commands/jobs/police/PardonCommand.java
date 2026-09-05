package io.github.brenoepics.roleplay.commands.jobs.police;

import static io.github.brenoepics.roleplay.features.job.JobsDelegate.getRoomUserShoutComposer;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class PardonCommand extends Command {

  public PardonCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) throws Exception {
    Habbo officer = gameClient.getHabbo();
    RpAvatar officerData = RolePlay.getAvatarManager().getRpAvatar(officer);

    if (!officerData.getJobRankEntity().hasPermission(JobPermissions.POLICE_ARREST)) {
      officer.whisper("Vous n'êtes pas policier.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!officerData.isDuty()) {
      officer.whisper("Vous devez être en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!officerData.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (params.length < 2) {
      officer.whisper(":gracier <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("pardon")
        .getTimeOut(officer.getHabboInfo().getId());
    if (timeout != null) {
      officer.whisper("Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
          .getEpochSecond() + " seconde(s) avant de réutiliser cette commande.");
      return true;
    }

    Habbo criminal = officer.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (criminal == null) {
      officer.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (criminal == officer) {
      officer.whisper("Vous ne pouvez pas vous gracier vous-même.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar criminalData = RolePlay.getAvatarManager().getRpAvatar(criminal);
    int userWantedStars = RolePlay.getWantedManager().getUserWantedStars(criminal.getHabboInfo().getId());
    if (!criminalData.isJailed() && userWantedStars <= 0) {
      officer.whisper("Ce joueur n'est ni emprisonné ni recherché.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getWantedManager().pardonCrimes(criminal);
    if (criminalData.isJailed()) {
      RolePlay.getPrisonService().releaseFromJail(criminal, criminalData);
    }

    officer.getHabboInfo().getCurrentRoom().sendComposer(getRoomUserShoutComposer(
        "* Efface le casier de %username% *".replace("%username%", criminal.getHabboInfo().getUsername()),
        officer, RoomChatMessageBubbles.AMBASSADOR).compose());
    officerData.executeAction();
    return true;
  }
}
