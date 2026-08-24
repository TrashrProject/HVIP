package io.github.brenoepics.roleplay.features.user.inventory;

/**
 * Helper class to represent a database inventory record
 */
public record DatabaseInventoryRecord(int slotIndex, int itemId, int quantity, int durability,
                                      boolean isDepositBox) {
}