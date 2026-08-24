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
      staff.whisper(":staffhit <player> <amount>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo victim = staff.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (victim == null) {
      staff.whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (victim == staff) {
      staff.whisper("You can't hit yourself!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar target = RolePlay.getAvatarManager().getRpAvatar(victim);
    if (target.isPassive()) {
      staff.whisper("You can't hit " + victim.getHabboInfo().getUsername()
          + " because they are in passive mode.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (target.isDead()) {
      staff.whisper(victim.getHabboInfo().getUsername() + " is already dead!",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    Integer damage = getDamage(params);
    if (damage == null) {
      staff.whisper("Invalid amount, must be a number", RoomChatMessageBubbles.ALERT);
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
