package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.Inventory;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;

public class DeleteInventoryItemCommand extends Command {

  public DeleteInventoryItemCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (params.length != 2) {
      gameClient.getHabbo().whisper(":supprimerobjet <case>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int slotIndex;
    try {
      slotIndex = Integer.parseInt(params[1]);
    } catch (NumberFormatException ignored) {
      gameClient.getHabbo().whisper("Case d'inventaire invalide.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (slotIndex < Inventory.REGULAR_SLOTS_START || slotIndex >= Inventory.TOTAL_SLOTS) {
      gameClient.getHabbo().whisper(
          "Range d'abord l'objet equipe avant de le supprimer.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
    if (avatar == null || avatar.getInventory() == null) {
      return true;
    }

    InventorySlot slot = avatar.getInventory().getSlot(slotIndex);
    if (slot == null || slot.isEmpty() || slot.getItem() == null) {
      gameClient.getHabbo().whisper("Cette case est vide.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String itemName = slot.getItem().getDisplayName();
    int quantity = slot.getQuantity();
    slot.clear();
    avatar.getInventory().updateInventory(gameClient.getHabbo());
    gameClient.getHabbo().whisper(
        quantity + " x " + itemName + " supprime(s) definitivement.",
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
