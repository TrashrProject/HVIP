package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.SHOOT_TIMEOUT;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import io.github.brenoepics.roleplay.utilities.types.WeaponProfile;

public class ShootCommand extends Command {

  public ShootCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo attacker = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(attacker);
    if (data.isPassive()) {
      attacker.whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 2) {
      attacker.whisper(":tirer <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.hasEnergy()) {
      attacker.whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    InventorySlot weaponSlot = data.getInventory().getPrimaryWeaponSlot();
    if (weaponSlot == null || weaponSlot.isEmpty() || !weaponSlot.isUsable()
        || weaponSlot.getItem() == null
        || !"weapon".equalsIgnoreCase(weaponSlot.getItem().getInteractionType())) {
      attacker.whisper("Vous n'avez pas équipé d'arme à feu.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RPItem weapon = weaponSlot.getItem();
    WeaponProfile weaponProfile = WeaponProfile.from(weapon);
    if (!weaponProfile.isRanged()) {
      attacker.whisper(weapon.getDisplayName() + " n'est pas une arme à feu.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = attacker.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (target == null) {
      attacker.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (target == attacker) {
      attacker.whisper("Vous ne pouvez pas vous tirer dessus.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int distanceX = Math.abs(target.getRoomUnit().getX() - attacker.getRoomUnit().getX());
    int distanceY = Math.abs(target.getRoomUnit().getY() - attacker.getRoomUnit().getY());
    int distance = Math.max(distanceX, distanceY);
    if (distance > weaponProfile.getRange()) {
      attacker.whisper("La cible est trop loin pour " + weapon.getDisplayName()
          + " (portée maximale : " + weaponProfile.getRange() + ").",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData.isPassive() && !targetData.isAggressive()) {
      attacker.whisper("Vous ne pouvez pas tirer sur " + target.getHabboInfo().getUsername()
          + " car ce joueur est en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (data.isAtSafeZone() && (!targetData.isAggressive() || !data.isAggressive())) {
      attacker.whisper("Vous ne pouvez pas utiliser cette commande dans une zone protégée.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!Emulator.getConfig().getBoolean("features.organizations.friendly_fire")
        && data.getOrganizationId() == targetData.getOrganizationId()) {
      attacker.whisper("Vous ne pouvez pas tirer sur " + target.getHabboInfo().getUsername()
          + " car vous appartenez à la même organisation.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("shoot")
        .getTimeOut(attacker.getHabboInfo().getId());
    if (timeout != null) {
      attacker.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de réutiliser cette commande.");
      return true;
    }

    if (targetData.isDead()) {
      attacker.whisper(target.getHabboInfo().getUsername() + " est déjà inconscient.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    int damage = weaponProfile.rollDamage();
    targetData.takeDamage(damage, attacker);

    String targetName = target.getHabboInfo().getUsername();
    if (targetData.isDead()) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(getMessage(
          "* Tire avec " + weapon.getDisplayName() + " sur " + targetName
              + " et le met K.-O. *", attacker));
    } else {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(getMessage(
          "* Tire avec " + weapon.getDisplayName() + " sur " + targetName + " et inflige "
              + damage + " dégât(s) *", attacker));
    }

    if (weaponProfile.getDurabilityLoss() > 0) {
      data.getInventory().decreaseWeaponDurability(attacker, weaponProfile.getDurabilityLoss());
      data.getInventory().updateInventory(attacker);
    }

    data.executeAction();
    RolePlay.getCommandsCounter().getCoolDown("shoot")
        .addTimeOut(attacker.getHabboInfo().getId(), SHOOT_TIMEOUT);
    return true;
  }

  private static ServerMessage getMessage(String message, Habbo hitter) {
    return new RoomUserShoutComposer(
        new RoomChatMessage(message, hitter, hitter, RoomChatMessageBubbles.NORMAL)).compose();
  }
}
