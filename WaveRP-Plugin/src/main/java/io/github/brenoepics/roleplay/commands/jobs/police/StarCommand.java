package io.github.brenoepics.roleplay.commands.jobs.police;

import static io.github.brenoepics.roleplay.features.job.JobsDelegate.getRoomUserShoutComposer;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import com.eu.habbo.habbohotel.users.HabboManager;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.wantedlist.Crime;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import org.jetbrains.annotations.Nullable;

public class StarCommand extends Command {

  public StarCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
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
      officer.whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length < 3) {
      officer.whisper(":rechercher <pseudo> <1-5>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("star")
        .getTimeOut(officer.getHabboInfo().getId());
    if (timeout != null) {
      officer.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de réutiliser cette commande.");
      return true;
    }

    Integer starCount = getStarCount(params, officer);
    if (starCount == null) return true;

    Crime manualCrime = RolePlay.getWantedManager().getCrimeByName("Recherche manuelle " + starCount);
    if (manualCrime == null) {
      officer.whisper("Les niveaux de recherche manuelle ne sont pas initialisés dans la base de données.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo criminal = Emulator.getGameEnvironment().getHabboManager().getHabbo(params[1]);
    HabboInfo criminalInfo = criminal != null
        ? criminal.getHabboInfo()
        : HabboManager.getOfflineHabboInfo(params[1]);

    if (criminalInfo == null) {
      officer.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (criminalInfo.getId() == officer.getHabboInfo().getId()) {
      officer.whisper("Vous ne pouvez pas vous ajouter un niveau de recherche.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getWantedManager().addCriminalRecord(
        criminalInfo.getId(), manualCrime, officer.getHabboInfo().getId());

    String action = "* Place " + criminalInfo.getUsername()
        + " au niveau de recherche " + starCount + " *";
    if (officer.getHabboInfo().getCurrentRoom() != null) {
      officer.getHabboInfo().getCurrentRoom().sendComposer(
          getRoomUserShoutComposer(action, officer, RoomChatMessageBubbles.AMBASSADOR).compose());
    }

    if (criminal != null) {
      criminal.whisper("Vous êtes désormais recherché niveau " + starCount + ".",
          RoomChatMessageBubbles.ALERT);
    }

    officer.whisper(criminalInfo.getUsername() + " est maintenant recherché niveau "
        + starCount + (criminal == null ? " (hors ligne)." : "."), RoomChatMessageBubbles.ALERT);

    officerData.executeAction();
    return true;
  }

  private static @Nullable Integer getStarCount(String[] params, Habbo officer) {
    int starCount;
    try {
      starCount = Integer.parseInt(params[2]);
      if (starCount < 1 || starCount > 5) {
        officer.whisper("Le niveau de recherche est invalide (1 à 5).",
            RoomChatMessageBubbles.ALERT);
        return null;
      }
    } catch (NumberFormatException exception) {
      officer.whisper("Le niveau de recherche est invalide (1 à 5).",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    return starCount;
  }
}
