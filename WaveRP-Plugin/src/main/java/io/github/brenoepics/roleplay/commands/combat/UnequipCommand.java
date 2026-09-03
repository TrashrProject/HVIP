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
      habbo.whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (params.length > 2) {
      habbo.whisper(":desequiper [arme|armure]", RoomChatMessageBubbles.ALERT);
      return true;
    }
    boolean armor = params.length == 2 && (params[1].equalsIgnoreCase("armure")
        || params[1].equalsIgnoreCase("armor") || params[1].equalsIgnoreCase("shield"));
    if (armor) {
      data.getInventory().unEquipArmor();
      data.setShield(0);
      data.updateLife();
    } else {
      data.getInventory().unEquipWeapon();
      habbo.getHabboInfo().getCurrentRoom().giveEffect(habbo, 0, -1);
      data.setEquippedWeapon(0);
    }
    data.getInventory().updateInventory(gameClient.getHabbo());
    habbo.whisper(armor ? "Vous avez rangé votre armure." : "Vous avez rangé votre arme.",
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
