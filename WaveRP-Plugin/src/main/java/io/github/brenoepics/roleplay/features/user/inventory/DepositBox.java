package io.github.brenoepics.roleplay.features.user.inventory;

import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.util.ArrayList;
import java.util.List;
import lombok.Getter;

@Getter
public class DepositBox {
  private final List<InventorySlot> slots;

  public DepositBox() {
    this.slots = new ArrayList<>();
  }

  public void addItem(RPItem item, int quantity) {
    int remainingQuantity = quantity;

    // Try to stack with existing items first
    for (InventorySlot slot : slots) {
      if (slot.canStackWith(item)) {
        remainingQuantity = slot.addItem(item, remainingQuantity);
        if (remainingQuantity <= 0) break;
      }
    }

    // Create new slots for the remaining quantity
    while (remainingQuantity > 0) {
      InventorySlot newSlot = new InventorySlot(false, "regular");
      remainingQuantity = newSlot.addItem(item, remainingQuantity);
      slots.add(newSlot);
    }
  }

  public boolean removeItem(RPItem item, int quantity) {
    int remainingToRemove = quantity;

    for (int i = slots.size() - 1; i >= 0; i--) {
      InventorySlot slot = slots.get(i);
      if (slot.getItem() != null && slot.getItem().equals(item)) {
        int removed = slot.removeItem(remainingToRemove);
        remainingToRemove -= removed;

        if (slot.isEmpty()) {
          slots.remove(i);
        }

        if (remainingToRemove <= 0) break;
      }
    }

    return remainingToRemove <= 0;
  }

  public int getTotalQuantity(RPItem item) {
    return slots.stream()
        .filter(slot -> slot.getItem() != null && slot.getItem().equals(item))
        .mapToInt(InventorySlot::getQuantity)
        .sum();
  }

  public boolean hasItem(RPItem item) {
    return getTotalQuantity(item) > 0;
  }

  public List<InventorySlot> getAllSlots() {
    return new ArrayList<>(slots);
  }

  public int getSlotCount() {
    return slots.size();
  }
}