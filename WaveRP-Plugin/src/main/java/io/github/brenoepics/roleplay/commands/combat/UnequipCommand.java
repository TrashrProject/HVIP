package io.github.brenoepics.roleplay.commands.combat;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class UnequipCommand extends Command {

  public UnequipCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data.isPassive()) {
      habbo.whisper("You cannot execute RolePlay commands while passive mode is on!",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (params.length != 1) {
      habbo.whisper(":unequip", RoomChatMessageBubbles.ALERT);
      return true;
    }
    data.getInventory().unEquipWeapon();
    data.getInventory().updateInventory(gameClient.getHabbo());
    habbo.getHabboInfo().getCurrentRoom().giveEffect(habbo, 0, -1);
    habbo.whisper("You have unequipped your current weapon", RoomChatMessageBubbles.ALERT);
    data.setEquippedWeapon(0);
    return true;
  }
}
