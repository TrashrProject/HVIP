package io.github.brenoepics.roleplay.features.user.inventory;

import io.github.brenoepics.roleplay.utilities.types.RPItem;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class InventorySlot {

  private RPItem item;
  private int quantity;
  private int durability; // For weapons and armor only
  private final int maxDurability = 100; // Default max durability
  private final boolean isSpecialSlot; // For primary weapon and secondary armor slots
  private final String slotType; // "weapon", "armor", "regular"

  public InventorySlot(boolean isSpecialSlot, String slotType) {
    this.isSpecialSlot = isSpecialSlot;
    this.slotType = slotType;
    this.quantity = 0;
    this.durability = maxDurability;
  }

  public boolean isEmpty() {
    return item == null || quantity <= 0;
  }

  public boolean isBroken() {
    return item != null && isDurabilityItem() && durability <= 0;
  }

  public boolean isUsable() {
    return !isEmpty() && !isBroken();
  }

  boolean isDurabilityItem() {
    return item != null && ("weapon".equals(item.getInteractionType()) || "shield".equals(
        item.getInteractionType()));
  }

  public boolean canStackWith(RPItem newItem) {
    if (isEmpty()) {
      return true;
    }
    if (!item.equals(newItem)) {
      return false;
    }

    // Weapons and armor don't stack
    String interactionType = item.getInteractionType();
    return !"weapon".equals(interactionType) && !"shield".equals(interactionType);
  }

  public boolean canAddItem(RPItem newItem) {
    if (isSpecialSlot) {
      // Primary weapon slot only accepts weapons
      if ("weapon".equals(slotType) && !"weapon".equals(newItem.getInteractionType())) {
        return false;
      }
      // Secondary armor slot only accepts armor/shields
      if ("armor".equals(slotType) && !"shield".equals(newItem.getInteractionType())) {
        return false;
      }
    }
    return canStackWith(newItem);
  }

  public int addItem(RPItem newItem, int quantityToAdd) {
    if (!canAddItem(newItem)) {
      return quantityToAdd; // Return all quantity as couldn't add any
    }

    if (isEmpty()) {
      this.item = newItem;
      this.quantity = quantityToAdd;
      this.durability = maxDurability; // New items start with full durability
      return 0; // Successfully added all
    }

    // For stackable items, check max stack size
    int maxStack = newItem.getMax() > 0 ? newItem.getMax() : Integer.MAX_VALUE;
    int canAdd = Math.min(quantityToAdd, maxStack - this.quantity);

    this.quantity += canAdd;
    return quantityToAdd - canAdd; // Return remaining quantity that couldn't be added
  }

  public int removeItem(int quantityToRemove) {
    if (isEmpty()) {
      return 0;
    }

    int removedQuantity = Math.min(quantityToRemove, this.quantity);
    this.quantity -= removedQuantity;

    if (this.quantity <= 0) {
      // Clear slot but keep reference for database tracking
      clear();
    }

    return removedQuantity;
  }

  public void decreaseDurability(int amount) {
    if (isDurabilityItem()) {
      this.durability = Math.max(0, this.durability - amount);
    }
  }

  public double getDurabilityPercentage() {
    if (!isDurabilityItem()) {
      return 1.0; // Non-durability items show as 100%
    }
    return (double) durability / maxDurability;
  }

  public void clear() {
    this.item = null;
    this.quantity = 0;
    this.durability = maxDurability;
  }

  public void markAsBroken() {
    if (isDurabilityItem()) {
      this.durability = 0;
    }
  }

  public void repair() {
    if (isDurabilityItem()) {
      this.durability = maxDurability;
    }
  }
}