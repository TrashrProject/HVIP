package io.github.brenoepics.roleplay.commands.generic;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.PASSIVE_TIMEOUT;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class PassiveCommand extends Command {

  public PassiveCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (params.length != 1) {
      gameClient.getHabbo().whisper(":passive", RoomChatMessageBubbles.ALERT);
      return true;
    }
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("passive")
        .getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
    if (timeout != null) {
      gameClient.getHabbo().whisper(
          "You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconds to use this command again!");
      return true;
    }

    if (data.isAggressive()) {
      long end = data.getAggressionUntil().minusMillis(System.currentTimeMillis()).getEpochSecond();
      gameClient.getHabbo().whisper(
          "You cannot execute RolePlay commands while aggressive mode is on! wait for" + end
              + " seconds to use this command again!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.isPassive()) {
      data.setDuty(false);
      data.setEquippedWeapon(0);
      gameClient.getHabbo().whisper("You have enabled passive mode.", RoomChatMessageBubbles.ALERT);
    } else {
      gameClient.getHabbo()
          .whisper("You have disabled passive mode.", RoomChatMessageBubbles.ALERT);
    }

    data.setPassive(!data.isPassive());
    RolePlay.getCommandsCounter().getCoolDown("passive")
        .addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), PASSIVE_TIMEOUT);
    return true;
  }
}
