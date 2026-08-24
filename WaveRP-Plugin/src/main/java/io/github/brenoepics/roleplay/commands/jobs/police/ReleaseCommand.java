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

public class ReleaseCommand extends Command {

  public ReleaseCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) throws Exception {
    Habbo officer = gameClient.getHabbo();
    RpAvatar officerData = RolePlay.getAvatarManager().getRpAvatar(officer);

    if (!officerData.getJobRankEntity().hasPermission(JobPermissions.POLICE_ARREST)) {
      officer.whisper("Vous n'etes pas policier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.isDuty()) {
      officer.whisper("Vous devez etre en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length < 2) {
      officer.whisper("Usage : :liberer <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("release")
        .getTimeOut(officer.getHabboInfo().getId());
    if (timeout != null) {
      officer.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de reutiliser cette commande.");
      return true;
    }

    Habbo criminal = com.eu.habbo.Emulator.getGameEnvironment().getHabboManager().getHabbo(params[1]);
    if (criminal == null) {
      officer.whisper("Ce joueur doit etre connecte pour etre libere.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (criminal == officer) {
      officer.whisper("Vous ne pouvez pas vous liberer vous-meme.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar criminalData = RolePlay.getAvatarManager().getRpAvatar(criminal);
    if (!criminalData.isJailed()) {
      officer.whisper("Ce joueur n'est pas en prison.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getPrisonService().releaseFromJail(criminal, criminalData);
    officer.getHabboInfo().getCurrentRoom().sendComposer(getRoomUserShoutComposer(
        "Libere %username% de prison *".replace("%username%",
            criminal.getHabboInfo().getUsername()), officer).compose());
    officerData.executeAction();
    return true;
  }
}
