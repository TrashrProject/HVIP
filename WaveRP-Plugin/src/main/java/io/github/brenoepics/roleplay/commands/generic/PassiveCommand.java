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
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
      return true;
    }

    if (data.isAggressive()) {
      long end = data.getAggressionUntil().minusMillis(System.currentTimeMillis()).getEpochSecond();
      gameClient.getHabbo().whisper(
          "Vous ne pouvez pas activer le mode passif pendant une agression. Attendez " + end
              + " seconde(s).", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.isPassive()) {
      data.setDuty(false);
      data.setEquippedWeapon(0);
      gameClient.getHabbo().whisper("Le mode passif est activ\u00e9.", RoomChatMessageBubbles.ALERT);
    } else {
      gameClient.getHabbo()
          .whisper("Le mode passif est d\u00e9sactiv\u00e9.", RoomChatMessageBubbles.ALERT);
    }

    data.setPassive(!data.isPassive());
    RolePlay.getCommandsCounter().getCoolDown("passive")
        .addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), PASSIVE_TIMEOUT);
    return true;
  }
}
