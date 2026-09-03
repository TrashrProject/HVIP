package io.github.brenoepics.roleplay.commands.combat;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.items.WeaponAmmoService;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import io.github.brenoepics.roleplay.utilities.types.WeaponProfile;

public class ReloadCommand extends Command {

  private static final int AMMO_ID = 6122;

  public ReloadCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (gameClient == null || gameClient.getHabbo() == null) {
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
    if (avatar == null) {
      return true;
    }

    if (avatar.isPassive()) {
      gameClient.getHabbo().whisper(
          "Vous ne pouvez pas recharger en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    InventorySlot weaponSlot = avatar.getInventory().getPrimaryWeaponSlot();
    if (weaponSlot == null || weaponSlot.isEmpty() || weaponSlot.getItem() == null) {
      gameClient.getHabbo().whisper(
          "Équipez d'abord une arme à feu.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RPItem weapon = weaponSlot.getItem();
    WeaponProfile profile = WeaponProfile.from(weapon);
    if (!profile.isRanged() || profile.getMagazineSize() <= 0) {
      gameClient.getHabbo().whisper(
          "Cette arme n'utilise pas de chargeur.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int userId = gameClient.getHabbo().getHabboInfo().getId();
    int currentAmmo = WeaponAmmoService.getAmmo(userId, weapon.getId(), profile.getMagazineSize());
    if (currentAmmo == WeaponAmmoService.ERROR) {
      gameClient.getHabbo().whisper(
          "Le système de munitions est indisponible.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (currentAmmo >= profile.getMagazineSize()) {
      gameClient.getHabbo().whisper(
          "Le chargeur est déjà plein (" + currentAmmo + "/" + profile.getMagazineSize() + ").",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    RPItem ammoItem = RolePlay.getItemManager().getItemById(AMMO_ID);
    InventorySlot ammoSlot = ammoItem == null ? null : avatar.getInventory().findItemSlot(ammoItem);
    if (ammoItem == null || ammoSlot == null || ammoSlot.isEmpty() || ammoSlot.getQuantity() <= 0) {
      gameClient.getHabbo().whisper(
          "Vous n'avez pas de Munitions dans votre inventaire.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int needed = profile.getMagazineSize() - currentAmmo;
    int used = Math.min(needed, ammoSlot.getQuantity());
    int updatedAmmo = WeaponAmmoService.addAmmo(userId, weapon.getId(), profile.getMagazineSize(), used);
    if (updatedAmmo == WeaponAmmoService.ERROR) {
      gameClient.getHabbo().whisper(
          "Impossible de recharger cette arme pour le moment.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    avatar.getInventory().removeItem(ammoItem, used);
    avatar.getInventory().updateInventory(gameClient.getHabbo());
    gameClient.getHabbo().whisper(
        "Vous rechargez " + weapon.getDisplayName() + " : " + updatedAmmo + "/"
            + profile.getMagazineSize() + " (" + used + " munition(s) utilisée(s)).",
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
