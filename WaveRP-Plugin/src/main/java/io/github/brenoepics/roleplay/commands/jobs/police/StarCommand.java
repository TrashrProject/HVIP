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
      officer.whisper("You are not an Police Officer", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.isDuty()) {
      officer.whisper("You are not on Duty", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!officerData.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length < 2) {
      officer.whisper(":star <user> 1-5", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("star")
        .getTimeOut(officer.getHabboInfo().getId());
    if (timeout != null) {
      officer.whisper(
          "You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconds to use this command again!");
      return true;
    }

    Habbo criminal = officer.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (criminal == null) {
      officer.whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (criminal == officer) {
      officer.whisper("You cannot add stars to yourself!", RoomChatMessageBubbles.ALERT);
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
    return new Crime(-1, "Manual charge", starCount, true, false, false,
        "Manual charge by " + officer.getHabboInfo().getUsername());
  }

  private static @Nullable Integer getStarCount(String[] params, Habbo officer) {
    int starCount;
    try {
      starCount = Integer.parseInt(params[2]);
      if (starCount < 1 || starCount > 5) {
        officer.whisper("Invalid number of stars", RoomChatMessageBubbles.ALERT);
        return null;
      }
    } catch (NumberFormatException exception) {
      officer.whisper("Invalid number of stars", RoomChatMessageBubbles.ALERT);
      return null;
    }
    return starCount;
  }
}
