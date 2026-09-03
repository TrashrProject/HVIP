package io.github.brenoepics.roleplay.communication.outgoing.roleplay;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.features.user.inventory.Inventory;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.RPItem;

public class OpenInventoryComposer extends OutgoingWebMessage {

  private static final String PARADISE_INVENTORY_BASE =
      "https://paradiserp.fr/nitro/inventory-items/";

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

    itemObj.addProperty("image_url", resolveImageUrl(item, baseUrl));

    return itemObj;
  }

  private String resolveImageUrl(RPItem item, String baseUrl) {
    String legacyFile = switch (item.getId()) {
      case 6101 -> "matraque.png";
      case 6102 -> "batte.png";
      case 6103 -> "epee.png";
      case 6104 -> "katana.png";
      case 6105 -> "hache-rouge.png";
      case 6106 -> "hache.png";
      case 6107 -> "epee-vip.png";
      case 6108 -> "hache-vip.png";
      case 6109 -> "usp-s.png";
      case 6110 -> "ak47.png";
      case 6111 -> "colete.png";
      case 6112 -> "sniper.png";
      case 6113 -> "mp5.png";
      case 6114 -> "reparo.png";
      case 6115 -> "vara.png";
      case 6116 -> "g36.png";
      case 6117 -> "akm.png";
      case 6118 -> "semente.png";
      case 6119 -> "atum.png";
      case 6120 -> "salmao.png";
      case 6121 -> "carrot.png";
      case 6122 -> "munitions.png";
      default -> null;
    };

    if (legacyFile != null) {
      return PARADISE_INVENTORY_BASE + legacyFile;
    }

    String configuredBase = baseUrl;
    if (configuredBase == null || configuredBase.isBlank() || configuredBase.contains("example.com")) {
      configuredBase = PARADISE_INVENTORY_BASE + "%item%.png";
    }
    return configuredBase.replace("%item%", item.getDisplayName());
  }
}
