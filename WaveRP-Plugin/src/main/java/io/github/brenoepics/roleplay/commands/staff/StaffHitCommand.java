package io.github.brenoepics.roleplay.commands.staff;

import static io.github.brenoepics.roleplay.commands.combat.HitCommand.hitUser;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import org.jetbrains.annotations.Nullable;

public class StaffHitCommand extends Command {

  public StaffHitCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();
    if (params.length <= 2) {
      staff.whisper(":frapperstaff <pseudo> <d\u00e9g\u00e2ts>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo victim = staff.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (victim == null) {
      staff.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (victim == staff) {
      staff.whisper("Vous ne pouvez pas vous frapper vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar target = RolePlay.getAvatarManager().getRpAvatar(victim);
    if (target.isPassive()) {
      staff.whisper("Vous ne pouvez pas frapper " + victim.getHabboInfo().getUsername()
          + " car ce joueur est en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (target.isDead()) {
      staff.whisper(victim.getHabboInfo().getUsername() + " est d\u00e9j\u00e0 inconscient.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    Integer damage = getDamage(params);
    if (damage == null) {
      staff.whisper("Le nombre de d\u00e9g\u00e2ts est invalide.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    hitUser(staff, target, damage, victim);
    return true;
  }

  private static @Nullable Integer getDamage(String[] params) {
    int damage;

    try {
      damage = Integer.parseInt(params[2]);
    } catch (NumberFormatException e) {
      return null;
    }
    return damage;
  }
}
