package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.Inventory;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.RPItem;

/** Staff-only helper used to validate the ParadiseRP combat/fishing/farming migration in game. */
public class RpTestKitCommand extends Command {

  private static final int MINIMUM_STAFF_RANK = 5;
  private static final int MAXIMUM_STAFF_RANK = 9;

  private static final int[][] KIT = {
      {6110, 1},  // AK47
      {6122, 25}, // Munitions
      {6111, 1},  // Gilet pare-balles
      {6114, 1},  // Kit de réparation
      {6115, 1},  // Canne à pêche
      {6118, 5}   // Graines
  };

  public RpTestKitCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    int rank = habbo.getHabboInfo().getRank().getId();
    if (rank < MINIMUM_STAFF_RANK || rank > MAXIMUM_STAFF_RANK) {
      habbo.whisper("Vous n'avez pas la permission d'utiliser cette commande.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (avatar == null) {
      habbo.whisper("Votre profil RP n'est pas chargé.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Inventory inventory = avatar.getInventory();
    StringBuilder missing = new StringBuilder();

    for (int[] entry : KIT) {
      RPItem item = RolePlay.getItemManager().getItemById(entry[0]);
      if (item == null) {
        if (missing.length() > 0) {
          missing.append(", ");
        }
        missing.append(entry[0]);
        continue;
      }
      grantToRegularInventory(inventory, item, entry[1]);
    }

    inventory.updateInventory(habbo);

    if (missing.length() > 0) {
      habbo.whisper("Kit RP ajouté partiellement. Items absents en BDD : " + missing,
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    habbo.whisper(
        "Kit RP ajouté : AK47, 25 munitions, gilet, réparation, canne à pêche et 5 graines.",
        RoomChatMessageBubbles.NORMAL);
    return true;
  }

  private static void grantToRegularInventory(Inventory inventory, RPItem item, int quantity) {
    int remaining = quantity;

    // Equipment slots 0/1 must only be populated by :equiper. Keep freshly granted items in
    // regular inventory so equip/unequip logic and visual effects remain authoritative.
    for (int i = Inventory.REGULAR_SLOTS_START; i < Inventory.TOTAL_SLOTS && remaining > 0; i++) {
      InventorySlot slot = inventory.getSlot(i);
      if (slot != null && !slot.isEmpty() && slot.canStackWith(item)) {
        remaining = slot.addItem(item, remaining);
      }
    }

    for (int i = Inventory.REGULAR_SLOTS_START; i < Inventory.TOTAL_SLOTS && remaining > 0; i++) {
      InventorySlot slot = inventory.getSlot(i);
      if (slot != null && slot.isEmpty()) {
        remaining = slot.addItem(item, remaining);
      }
    }

    if (remaining > 0) {
      inventory.getDepositBox().addItem(item, remaining);
    }
  }
}
