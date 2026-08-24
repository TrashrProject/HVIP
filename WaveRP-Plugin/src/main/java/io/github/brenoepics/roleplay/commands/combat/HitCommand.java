package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.HIT_TIMEOUT;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ThreadLocalRandom;
import org.jetbrains.annotations.NotNull;

public class HitCommand extends Command {

  public static final int DECREASE_DURABILITY_AMOUNT = 1;

  public HitCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  public boolean handle(GameClient gameClient, String[] params) {
    Habbo attacker = gameClient.getHabbo();
    RpAvatar attackerData = RolePlay.getAvatarManager().getRpAvatar(attacker);
    if (attackerData.isPassive()) {
      attacker.whisper("Sorry you can not use this command while in passive mode.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 2) {
      attacker.whisper(":hit <player>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!attackerData.hasEnergy()) {
      attacker.whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo habbo = attacker.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (habbo == null) {
      attacker.whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (habbo == attacker) {
      attacker.whisper("You can't hit yourself!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (targetData.isPassive() && !targetData.isAggressive()) {
      attacker.whisper("You can't hit " + habbo.getHabboInfo().getUsername()
          + " because they are in passive mode.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (attackerData.isAtSafeZone() && (!targetData.isAggressive()
        || !attackerData.isAggressive())) {
      attacker.whisper("Sorry you can not use this command while in a safe zone.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!Emulator.getConfig().getBoolean("features.organizations.friendly_fire")
        && attackerData.getOrganizationId() == targetData.getOrganizationId()) {
      attacker.whisper("You can't hit " + habbo.getHabboInfo().getUsername()
          + " because they are in the same organization as you.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("hit")
        .getTimeOut(attacker.getHabboInfo().getId());
    if (timeout != null) {
      attacker.whisper(
          "You must wait " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconds to swing again");
      return true;
    }

    List<RoomTile> tiles = getAround(habbo);
    List<RoomTile> nonDiag = getTilesAround(habbo);
    RoomTile middleTile = habbo.getRoomUnit().getCurrentLocation();

    List<RoomTile> tilesCheck1 = getRoomTiles(tiles, nonDiag, middleTile, habbo);

    if (!isAttackAllowed(attacker, habbo, nonDiag, middleTile)) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(
          createKnockoutMessage("swings at " + habbo.getHabboInfo().getUsername() + ", but misses*", attacker).compose());
      RolePlay.getCommandsCounter().getCoolDown("hit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    if (!tilesCheck1.contains(attacker.getRoomUnit().getCurrentLocation())) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(
          createKnockoutMessage("swings at " + habbo.getHabboInfo().getUsername() + ", but misses*",
              attacker).compose());
      // Apply cooldown on miss
      RolePlay.getCommandsCounter().getCoolDown("hit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    if (targetData.isDead()) {
      attacker.whisper(habbo.getHabboInfo().getUsername() + " is already dead!",
          RoomChatMessageBubbles.ALERT);
      // Apply cooldown even if target is dead
      RolePlay.getCommandsCounter().getCoolDown("hit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    int damage = ThreadLocalRandom.current().nextInt(1, 4);
    Optional<InventorySlot> equippedWeapon = attackerData.getEquippedWeapon();
    if (equippedWeapon.isPresent()) {
      damage = getDamage(equippedWeapon.get().getItem());
      equippedWeapon.get().decreaseDurability(DECREASE_DURABILITY_AMOUNT);
    }

    attackerData.getInventory().decreaseWeaponDurability(attacker, DECREASE_DURABILITY_AMOUNT);
    damage = (int) Math.round(damage * attackerData.getStrength());
    hitUser(attacker, targetData, damage, habbo);
    attackerData.getCombatStats().recordPunchThrown();
    attackerData.executeAction();
    // Apply cooldown on hit
    RolePlay.getCommandsCounter().getCoolDown("hit")
        .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
    return true;
  }

  private static boolean isAttackAllowed(Habbo attacker, Habbo target, List<RoomTile> nonDiag, RoomTile middleTile) {
    RoomTile attackerTile = attacker.getRoomUnit().getCurrentLocation();
    // If attacker is on the same or non-diagonal tile, allow
    if (attackerTile.equals(middleTile) || nonDiag.contains(attackerTile)) {
      return true;
    }
    // If attacker is on a diagonal tile
    boolean attackerWalking = attacker.getRoomUnit().isWalking();
    boolean targetWalking = target.getRoomUnit().isWalking();
    // Check if both are walking and are adjacent diagonally
    return attackerWalking && targetWalking && isAdjacentDiagonal(attackerTile, middleTile);
    // Otherwise, diagonal attack not allowed
  }

  private static boolean isAdjacentDiagonal(RoomTile a, RoomTile b) {
    int dx = Math.abs(a.x - b.x);
    int dy = Math.abs(a.y - b.y);
    return dx == 1 && dy == 1;
  }

  private static int getDamage(RPItem item) {
    String data = item.getExtraData();
    if (data == null || data.isEmpty()) {
      return getDefaultDamage();
    }
    try {
      if (data.contains(",")) {
        String[] parts = data.split(",");
        int min = Integer.parseInt(parts[0].trim());
        int max = Integer.parseInt(parts[1].trim());
        if (min > max) { // swap if out of order
          int temp = min;
          min = max;
          max = temp;
        }
        return ThreadLocalRandom.current().nextInt(min, max + 1);
      } else {
        return Integer.parseInt(data.trim());
      }
    } catch (NumberFormatException e) {
      return getDefaultDamage();
    }
  }

  private static int getDefaultDamage() {
    return ThreadLocalRandom.current().nextInt(1, 4);
  }

  private static boolean isNotAround(List<RoomTile> diagOnly, Habbo hitter, RoomTile middleTile) {
    if (hitter.getRoomUnit().getCurrentLocation().equals(middleTile)) {
      return false;
    }
    if (!diagOnly.contains(hitter.getRoomUnit().getCurrentLocation())) {
      return false;
    }
    return !hitter.getRoomUnit().isWalking() || (!hitter.getRoomUnit().isFastWalk() && (
        !hitter.getRoomUnit().getCurrentLocation().equals(middleTile) || !hitter.getRoomUnit()
            .getPreviousLocation().equals(middleTile) || !hitter.getRoomUnit().getStartLocation()
            .equals(middleTile)));
  }

  private static List<RoomTile> getAround(Habbo habbo) {
    return habbo.getHabboInfo().getCurrentRoom().getLayout()
        .getTilesAround(habbo.getRoomUnit().getCurrentLocation(), 0, true);
  }

  private static List<RoomTile> getTilesAround(Habbo habbo) {
    return habbo.getHabboInfo().getCurrentRoom().getLayout()
        .getTilesAround(habbo.getRoomUnit().getCurrentLocation(), 0, false);
  }

  private static @NotNull List<RoomTile> getRoomTiles(List<RoomTile> tiles, List<RoomTile> nonDiag,
      RoomTile middleTile, Habbo habbo) {
    List<RoomTile> tilesCheck1 = new ArrayList<>();
    tilesCheck1.addAll(tiles);
    tilesCheck1.addAll(nonDiag);
    tiles.add(middleTile);
    tilesCheck1.add(middleTile);
    tilesCheck1.add(habbo.getRoomUnit().getStartLocation());
    tilesCheck1.add(habbo.getRoomUnit().getPreviousLocation());
    return tilesCheck1;
  }

  public static void hitUser(Habbo actor, RpAvatar targetData, int damage, Habbo habbo) {
    targetData.takeDamage(damage, actor);
    String victimUsername = habbo.getHabboInfo().getUsername();
    targetData.getCombatStats().recordPunchReceived();

    if (targetData.isDead()) {
      actor.getHabboInfo().getCurrentRoom().sendComposer(createKnockoutMessage(
          "Lands the final blow on " + victimUsername + ", knocking them out*", actor).compose());
    } else {
      actor.getHabboInfo().getCurrentRoom().sendComposer(
          createKnockoutMessage("Swings at " + victimUsername + " dealing " + damage + " damage*",
              actor).compose());
    }
  }

  private static RoomUserShoutComposer createKnockoutMessage(String habbo, Habbo hitter) {
    return new RoomUserShoutComposer(
        new RoomChatMessage(habbo, hitter, hitter, RoomChatMessageBubbles.NORMAL));
  }
}
