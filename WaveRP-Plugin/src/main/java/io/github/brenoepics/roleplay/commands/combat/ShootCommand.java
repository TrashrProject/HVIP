package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.SHOOT_TIMEOUT;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import java.util.concurrent.ThreadLocalRandom;

public class ShootCommand extends Command {

  public ShootCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo attacker = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(attacker);
    if (data.isPassive()) {
      attacker.whisper("You cannot execute RolePlay commands while passive mode is on!",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 2) {
      attacker.whisper(":shoot <player>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.hasEnergy()) {
      gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo habbo = attacker.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    RoomUnit roomUnit = attacker.getRoomUnit();
    if (roomUnit.getEffectId() != 164) {
      attacker.whisper("You dont have a pistol equipped", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (habbo == null) {
      attacker.whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (habbo == attacker) {
      attacker.whisper("You cannot shoot yourself!", RoomChatMessageBubbles.ALERT);
      return true;
    }
    int distanceX = habbo.getRoomUnit().getX() - attacker.getRoomUnit().getX();
    int distanceY = habbo.getRoomUnit().getY() - attacker.getRoomUnit().getY();

    if (distanceX < -3 || distanceX > 3 || distanceY < -3 || distanceY > 3) {
      attacker.whisper(Emulator.getTexts().getValue("commands.error.cmd_pull.cant_reach")
          .replace("%user%", params[1]), RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (targetData.isPassive() && !targetData.isAggressive()) {
      gameClient.getHabbo().whisper("You can't hit " + habbo.getHabboInfo().getUsername()
          + " because they are in passive mode.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (data.isAtSafeZone() && (!targetData.isAggressive() || !data.isAggressive())) {
      gameClient.getHabbo().whisper("Sorry you can not use this command while in a safe zone.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!Emulator.getConfig().getBoolean("features.organizations.friendly_fire")
        && data.getOrganizationId() == targetData.getOrganizationId()) {
      attacker.whisper("You can't shoot " + habbo.getHabboInfo().getUsername()
          + " because they are in the same organization as you.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("shoot")
        .getTimeOut(attacker.getHabboInfo().getId());
    if (timeout != null) {
      attacker.whisper(
          "You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconds to use this command again!");
      return true;
    }

    if (targetData.isDead()) {
      attacker.whisper(habbo.getHabboInfo().getUsername() + " is already dead!",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    int damage = ThreadLocalRandom.current().nextInt(25, 35 + 1);
    targetData.takeDamage(damage, attacker);
    if (targetData.isDead()) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(getMessage(
          "Lands the final blow on " + habbo.getHabboInfo().getUsername() + ", knocking them out*",
          attacker));
    } else {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(getMessage(
          "Shoots at " + habbo.getHabboInfo().getUsername() + " and causes " + damage + " damage*",
          attacker));
    }

    data.executeAction();
    RolePlay.getCommandsCounter().getCoolDown("shoot")
        .addTimeOut(attacker.getHabboInfo().getId(), SHOOT_TIMEOUT);
    return true;
  }

  private static ServerMessage getMessage(String habbo, Habbo hitter) {
    return new RoomUserShoutComposer(
        new RoomChatMessage(habbo, hitter, hitter, RoomChatMessageBubbles.NORMAL)).compose();
  }
}
