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
      habbo.whisper(":sendhome <user> <minutes>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = habbo.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (target == null) {
      habbo.whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData == null) {
      habbo.whisper("RP Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (target == habbo) {
      habbo.whisper("You cannot send yourself home!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetData.getJobEntity() == null || targetData.getJobEntity()
        .equals(RolePlay.getJobService().getUnemployedJob())) {
      habbo.whisper("Player " + params[1] + " is not employed!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetData.getJobEntity().getId() != data.getJobEntity().getId()) {
      habbo.whisper("Player " + params[1] + " is not in the same job as you!",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (targetData.getJobRankEntity().isHigherOrEqualThan(data.getJobRankEntity())) {
      habbo.whisper("You cannot send someone with the same or higher rank to home!",
          RoomChatMessageBubbles.ALERT);
    }

    int minutes = DEFAULT_SEND_HOME_TIME;

    if (params.length == 3) {
      try {
        minutes = Integer.parseInt(params[2]);
      } catch (Exception e) {
        habbo.whisper("Invalid time!", RoomChatMessageBubbles.ALERT);
        return true;
      }
    }

    if (minutes < SEND_HOME_MIN || minutes > SEND_HOME_MAX) {
      habbo.whisper("You can only send a user home for " + SEND_HOME_MIN + " to " + SEND_HOME_MAX
          + " minutes!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getJobsManager().getWorkCountDown()
        .getTimeOut(habbo.getHabboInfo().getId());
    if (timeout != null) {
      habbo.whisper(
          "You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconds to use this command again!");
      return true;
    }

    if (!data.isDuty()) {
      gameClient.getHabbo().whisper("You are not on duty!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getJobsManager().sendHome(targetData, target, habbo, minutes);
    return true;
  }
}
