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
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import io.github.brenoepics.roleplay.utilities.types.WeaponProfile;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ThreadLocalRandom;
import org.jetbrains.annotations.NotNull;

public class HitCommand extends Command {

  public HitCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  public boolean handle(GameClient gameClient, String[] params) {
    Habbo attacker = gameClient.getHabbo();
    RpAvatar attackerData = RolePlay.getAvatarManager().getRpAvatar(attacker);
    if (attackerData.isPassive()) {
      attacker.whisper("Vous ne pouvez pas utiliser cette commande en mode passif.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 2) {
      attacker.whisper(":frapper <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!attackerData.hasEnergy()) {
      attacker.whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo habbo = attacker.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (habbo == null) {
      attacker.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (habbo == attacker) {
      attacker.whisper("Vous ne pouvez pas vous frapper vous-même.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (targetData.isPassive() && !targetData.isAggressive()) {
      attacker.whisper("Vous ne pouvez pas frapper " + habbo.getHabboInfo().getUsername()
          + " car ce joueur est en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (attackerData.isAtSafeZone() && (!targetData.isAggressive()
        || !attackerData.isAggressive())) {
      attacker.whisper("Vous ne pouvez pas utiliser cette commande dans une zone protégée.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!Emulator.getConfig().getBoolean("features.organizations.friendly_fire")
        && attackerData.getOrganizationId() == targetData.getOrganizationId()) {
      attacker.whisper("Vous ne pouvez pas frapper " + habbo.getHabboInfo().getUsername()
          + " car vous appartenez à la même organisation.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("hit")
        .getTimeOut(attacker.getHabboInfo().getId());
    if (timeout != null) {
      attacker.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de frapper de nouveau.");
      return true;
    }

    List<RoomTile> tiles = getAround(habbo);
    List<RoomTile> nonDiag = getTilesAround(habbo);
    RoomTile middleTile = habbo.getRoomUnit().getCurrentLocation();

    List<RoomTile> tilesCheck1 = getRoomTiles(tiles, nonDiag, middleTile, habbo);

    if (!isAttackAllowed(attacker, habbo, nonDiag, middleTile)) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(
          createKnockoutMessage("* Frappe " + habbo.getHabboInfo().getUsername() + ", mais le rate *", attacker).compose());
      RolePlay.getCommandsCounter().getCoolDown("hit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    if (!tilesCheck1.contains(attacker.getRoomUnit().getCurrentLocation())) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(
          createKnockoutMessage("* Frappe " + habbo.getHabboInfo().getUsername() + ", mais le rate *",
              attacker).compose());
      RolePlay.getCommandsCounter().getCoolDown("hit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    if (targetData.isDead()) {
      attacker.whisper(habbo.getHabboInfo().getUsername() + " est déjà inconscient.",
          RoomChatMessageBubbles.ALERT);
      RolePlay.getCommandsCounter().getCoolDown("hit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    int damage = ThreadLocalRandom.current().nextInt(1, 4);
    Optional<InventorySlot> equippedWeapon = attackerData.getEquippedWeapon();
    if (equippedWeapon.isPresent() && equippedWeapon.get().getItem() != null) {
      WeaponProfile profile = WeaponProfile.from(equippedWeapon.get().getItem());
      // Firearms are handled by :tirer. A firearm in hand must not silently turn
      // :frapper into a ranged-damage melee attack.
      if (!profile.isRanged()) {
        damage = profile.rollDamage();
        if (profile.getDurabilityLoss() > 0) {
          attackerData.getInventory().decreaseWeaponDurability(attacker,
              profile.getDurabilityLoss());
          attackerData.getInventory().updateInventory(attacker);
        }
      }
    }

    damage = (int) Math.round(damage * attackerData.getStrength());
    hitUser(attacker, targetData, damage, habbo);
    attackerData.getCombatStats().recordPunchThrown();
    attackerData.executeAction();
    RolePlay.getCommandsCounter().getCoolDown("hit")
        .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
    return true;
  }

  private static boolean isAttackAllowed(Habbo attacker, Habbo target, List<RoomTile> nonDiag,
      RoomTile middleTile) {
    RoomTile attackerTile = attacker.getRoomUnit().getCurrentLocation();
    if (attackerTile.equals(middleTile) || nonDiag.contains(attackerTile)) {
      return true;
    }
    boolean attackerWalking = attacker.getRoomUnit().isWalking();
    boolean targetWalking = target.getRoomUnit().isWalking();
    return attackerWalking && targetWalking && isAdjacentDiagonal(attackerTile, middleTile);
  }

  private static boolean isAdjacentDiagonal(RoomTile a, RoomTile b) {
    int dx = Math.abs(a.x - b.x);
    int dy = Math.abs(a.y - b.y);
    return dx == 1 && dy == 1;
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
          "* Porte le coup final à " + victimUsername + " et le met K.-O. *", actor).compose());
    } else {
      actor.getHabboInfo().getCurrentRoom().sendComposer(
          createKnockoutMessage("* Frappe " + victimUsername + " et inflige " + damage + " dégât(s) *",
              actor).compose());
    }
  }

  private static RoomUserShoutComposer createKnockoutMessage(String message, Habbo hitter) {
    return new RoomUserShoutComposer(
        new RoomChatMessage(message, hitter, hitter, RoomChatMessageBubbles.NORMAL));
  }
}
