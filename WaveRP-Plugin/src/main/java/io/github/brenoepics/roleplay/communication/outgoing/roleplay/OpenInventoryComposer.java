package io.github.brenoepics.roleplay.communication.outgoing.roleplay;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.features.user.inventory.Inventory;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.RPItem;

public class OpenInventoryComposer extends OutgoingWebMessage {

  public OpenInventoryComposer(Inventory inventory, String baseUrl, boolean shouldOpen) {
    super("open_inventory");

    JsonArray inventoryData = new JsonArray();

    // Process inventory slots only
    InventorySlot[] slots = inventory.getAllSlots();
    for (int i = 0; i < slots.length; i++) {
      InventorySlot slot = slots[i];
      if (!slot.isEmpty()) {
        JsonObject itemObj = createItemJson(slot, i, baseUrl);
        itemObj.addProperty("slot_index", i);
        itemObj.addProperty("is_special_slot", slot.isSpecialSlot());
        itemObj.addProperty("slot_type", slot.getSlotType());

        // Mark if it's equipped in special slots
        switch (i) {
          case Inventory.PRIMARY_WEAPON_SLOT -> {
            itemObj.addProperty("equipped", true);
            itemObj.addProperty("equipment_type", "primary_weapon");
          }
          case Inventory.SECONDARY_ARMOR_SLOT -> {
            itemObj.addProperty("equipped", true);
            itemObj.addProperty("equipment_type", "secondary_armor");
          }
          default -> itemObj.addProperty("equipped", false);
        }

        inventoryData.add(itemObj);
      }
    }

    this.data.add("inventory_slots", inventoryData);
    this.data.addProperty("total_inventory_slots", Inventory.TOTAL_SLOTS);
    this.data.addProperty("primary_weapon_slot", Inventory.PRIMARY_WEAPON_SLOT);
    this.data.addProperty("secondary_armor_slot", Inventory.SECONDARY_ARMOR_SLOT);
    this.data.addProperty("should_open", shouldOpen);
  }

  private JsonObject createItemJson(InventorySlot slot, int slotIndex, String baseUrl) {
    RPItem item = slot.getItem();
    JsonObject itemObj = new JsonObject();

    // Basic item information
    itemObj.addProperty("item_id", item.getId());
    itemObj.addProperty("display_name", item.getDisplayName());
    itemObj.addProperty("quantity", slot.getQuantity());

    // Durability information (for weapons and armor)
    boolean hasDurability =
        "weapon".equals(item.getInteractionType()) || "shield".equals(item.getInteractionType());
    itemObj.addProperty("has_durability", hasDurability);
    if (hasDurability) {
      itemObj.addProperty("durability", slot.getDurability());
      itemObj.addProperty("max_durability", 100);
      itemObj.addProperty("durability_percentage", slot.getDurabilityPercentage() * 100);
      itemObj.addProperty("is_broken", slot.getDurability() <= 0);
    }

    // Image URL
    itemObj.addProperty("image_url", baseUrl.replace("%item%", item.getDisplayName()));

    return itemObj;
  }
}