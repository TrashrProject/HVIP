package io.github.brenoepics.roleplay.communication.outgoing.roleplay;

import com.google.gson.JsonArray;
import com.google.gson.JsonObject;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.features.user.inventory.DepositBox;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.types.FoodPresentation;
import io.github.brenoepics.roleplay.utilities.types.RPItem;

public class OpenDepositBoxComposer extends OutgoingWebMessage {

  public OpenDepositBoxComposer(DepositBox depositBox, String baseUrl) {
    super("open_deposit_box");

    JsonArray depositBoxData = new JsonArray();

    // Process deposit box items
    var depositSlots = depositBox.getAllSlots();
    for (int i = 0; i < depositSlots.size(); i++) {
      InventorySlot slot = depositSlots.get(i);
      if (!slot.isEmpty()) {
        JsonObject itemObj = createItemJson(slot, baseUrl);
        itemObj.addProperty("slot_index", i);
        itemObj.addProperty("slot_type", "deposit");

        depositBoxData.add(itemObj);
      }
    }

    this.data.add("deposit_box_slots", depositBoxData);
    this.data.addProperty("total_deposit_slots", depositSlots.size());
  }

  private JsonObject createItemJson(InventorySlot slot, String baseUrl) {
    RPItem item = slot.getItem();
    JsonObject itemObj = new JsonObject();

    // Basic item information
    itemObj.addProperty("item_id", item.getId());
    itemObj.addProperty("item_name", item.getDisplayName());
    itemObj.addProperty("display_name", item.getDisplayName().toLowerCase());
    itemObj.addProperty("quantity", slot.getQuantity());

    // Item properties
    itemObj.addProperty("interaction_type", item.getInteractionType());
    itemObj.addProperty("max_stack", item.getMax());
    itemObj.addProperty("price", item.getPrice());
    itemObj.addProperty("enable_id", item.getEnableId());
    itemObj.addProperty("extra_data", item.getExtraData());

    // Job requirements
    itemObj.addProperty("required_job", item.getRequirementJob() != null ? item.getRequirementJob().getName() : null);
    itemObj.addProperty("offer_job", item.getOfferJob() != null ? item.getOfferJob().getName() : null);

    // Durability information (for weapons and armor)
    boolean hasDurability = "weapon".equals(item.getInteractionType()) || "shield".equals(item.getInteractionType());
    itemObj.addProperty("has_durability", hasDurability);
    if (hasDurability) {
      itemObj.addProperty("durability", slot.getDurability());
      itemObj.addProperty("max_durability", 100);
      itemObj.addProperty("durability_percentage", slot.getDurabilityPercentage() * 100);
      itemObj.addProperty("is_broken", slot.getDurability() <= 0);
    }

    // Interaction type flags for easy client-side handling
    itemObj.addProperty("is_weapon", "weapon".equals(item.getInteractionType()));
    itemObj.addProperty("is_armor", "shield".equals(item.getInteractionType()));
    itemObj.addProperty("is_food", "food".equals(item.getInteractionType()));
    itemObj.addProperty("is_drug", "drug".equals(item.getInteractionType()));
    itemObj.addProperty("is_heal", "heal".equals(item.getInteractionType()));
    itemObj.addProperty("is_energy", "energy".equals(item.getInteractionType()));
    itemObj.addProperty("is_consumable", isConsumable(item.getInteractionType()));
    itemObj.addProperty("is_stackable", isStackable(item.getInteractionType()));

    // Image URL
    String foodImage = FoodPresentation.imageUrl(item);
    itemObj.addProperty("image_url",
        foodImage != null ? foodImage : baseUrl.replace("%item%", item.getDisplayName()));

    // Permission requirements
    itemObj.addProperty("permission_required", item.getPermission());

    // Organization crafting information
    JsonArray crafterOrgs = new JsonArray();
    if (item.getCrafterOrganizations() != null) {
      item.getCrafterOrganizations().forEach(org -> crafterOrgs.add(org.name().toLowerCase()));
    }
    itemObj.add("crafter_organizations", crafterOrgs);

    return itemObj;
  }

  private boolean isConsumable(String interactionType) {
    return switch (interactionType) {
      case "food", "drug", "heal", "energy" -> true;
      default -> false;
    };
  }

  private boolean isStackable(String interactionType) {
    return switch (interactionType) {
      case "weapon", "shield" -> false; // Weapons and armor don't stack
      default -> true;
    };
  }
}
