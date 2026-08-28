package io.github.brenoepics.roleplay.commands.jobs.police;

import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.wantedlist.Crime;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import org.jetbrains.annotations.NotNull;
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
      officer.whisper("Vous n'\u00eates pas policier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.isDuty()) {
      officer.whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length < 2) {
      officer.whisper(":recherche <pseudo> 1-5", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("star")
        .getTimeOut(officer.getHabboInfo().getId());
    if (timeout != null) {
      officer.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
      return true;
    }

    Habbo criminal = officer.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (criminal == null) {
      officer.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (criminal == officer) {
      officer.whisper("Vous ne pouvez pas vous ajouter un niveau de recherche.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Integer starCount = getStarCount(params, officer);
    if (starCount == null) {
      return true;
    }

    RolePlay.getWantedManager().chargeCrime(officer, criminal, getManualCharge(starCount, officer));
    officerData.executeAction();
    return true;
  }

  private static @NotNull Crime getManualCharge(Integer starCount, Habbo officer) {
    return new Crime(-1, "Inculpation manuelle", starCount, true, false, false,
        "Inculpation manuelle par " + officer.getHabboInfo().getUsername());
  }

  private static @Nullable Integer getStarCount(String[] params, Habbo officer) {
    int starCount;
    try {
      starCount = Integer.parseInt(params[2]);
      if (starCount < 1 || starCount > 5) {
        officer.whisper("Le niveau de recherche est invalide.", RoomChatMessageBubbles.ALERT);
        return null;
      }
    } catch (NumberFormatException exception) {
      officer.whisper("Le niveau de recherche est invalide.", RoomChatMessageBubbles.ALERT);
      return null;
    }
    return starCount;
  }
}
