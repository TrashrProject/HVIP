package io.github.brenoepics.roleplay.features.user.inventory;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.outgoing.roleplay.OpenInventoryComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import lombok.Getter;
import lombok.extern.slf4j.Slf4j;

@Slf4j
@Getter
public class Inventory {

  public static final int TOTAL_SLOTS = 12;
  public static final int PRIMARY_WEAPON_SLOT = 0;
  public static final int SECONDARY_ARMOR_SLOT = 1;
  public static final int REGULAR_SLOTS_START = 2;

  private final InventorySlot[] slots;
  private final DepositBox depositBox;

  public Inventory() {
    this.slots = new InventorySlot[TOTAL_SLOTS];
    this.depositBox = new DepositBox();

    // Initialize slots
    for (int i = 0; i < TOTAL_SLOTS; i++) {
      switch (i) {
        case PRIMARY_WEAPON_SLOT -> slots[i] = new InventorySlot(true, "weapon");
        case SECONDARY_ARMOR_SLOT -> slots[i] = new InventorySlot(true, "armor");
        default -> slots[i] = new InventorySlot(false, "regular");
      }
    }
  }

  public static void createInventory(Habbo habbo, Connection connection, RpAvatar data) {
    try (PreparedStatement selectStmt = connection.prepareStatement(
        "SELECT * FROM user_inventory WHERE user_id = ?")) {
      selectStmt.setInt(1, habbo.getHabboInfo().getId());
      ResultSet resultSet = selectStmt.executeQuery();
      while (resultSet.next()) {
        int itemId = resultSet.getInt("item_id");
        int quantity = resultSet.getInt("quantity");
        int durability = resultSet.getInt("durability");
        int slotIndex = resultSet.getInt("slot_index");
        boolean isDepositBox = resultSet.getBoolean("is_deposit_box");

        RPItem item = RolePlay.getItemManager().getItems().get(itemId);
        if (item == null) {
          log.error("User: {} has an item with ID {} not found in the database!",
              habbo.getHabboInfo().getId(), itemId);
          continue;
        }

        if (isDepositBox) {
          // Add to the deposit box
          InventorySlot depositSlot = new InventorySlot(false, "regular");
          depositSlot.setItem(item);
          depositSlot.setQuantity(quantity);
          depositSlot.setDurability(durability);
          data.getInventory().getDepositBox().getSlots().add(depositSlot);
        } else {
          // Add to the inventory slot
          InventorySlot slot = data.getInventory().getSlot(slotIndex);
          if (slot != null) {
            slot.setItem(item);
            slot.setQuantity(quantity);
            slot.setDurability(durability);
          }
        }
      }
    } catch (SQLException e) {
      log.error("Error while loading RolePlay inventory data for {}",
          habbo.getHabboInfo().getUsername(), e);
    }

    if (data.isPassive()) {
      data.setEquippedWeapon(0);
    }
  }

  public boolean addItem(Habbo habbo, RPItem item, int quantity) {
    int remainingQuantity = quantity;

    // First, try to add to inventory slots
    remainingQuantity = addToInventorySlots(item, remainingQuantity);

    // If there's a remaining quantity, add to the deposit box
    if (remainingQuantity > 0) {
      depositBox.addItem(item, remainingQuantity);
    }

    updateInventory(habbo);
    return true; // Always successful (deposit box has unlimited space)
  }

  private int addToInventorySlots(RPItem item, int quantity) {
    int remainingQuantity = quantity;

    // Try to stack with existing items first
    for (InventorySlot slot : slots) {
      if (slot.canStackWith(item)) {
        remainingQuantity = slot.addItem(item, remainingQuantity);
        if (remainingQuantity <= 0) {
          break;
        }
      }
    }

    // Try to add to empty slots
    if (remainingQuantity > 0) {
      for (InventorySlot slot : slots) {
        if (slot.isEmpty() && slot.canAddItem(item)) {
          remainingQuantity = slot.addItem(item, remainingQuantity);
          if (remainingQuantity <= 0) {
            break;
          }
        }
      }
    }

    return remainingQuantity;
  }

  public boolean removeItem(RPItem item, int quantity) {
    int remainingToRemove = quantity;

    // First, try to remove from inventory slots
    for (int i = slots.length - 1; i >= 0; i--) {
      InventorySlot slot = slots[i];
      if (slot.getItem() != null && slot.getItem().equals(item)) {
        int removed = slot.removeItem(remainingToRemove);
        remainingToRemove -= removed;
        if (remainingToRemove <= 0) {
          break;
        }
      }
    }

    // If still need to remove more, try deposit box
    if (remainingToRemove > 0) {
      depositBox.removeItem(item, remainingToRemove);
    }

    return true;
  }

  public void updateInventory(Habbo habbo) {
    if (habbo == null) {
      return;
    }
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    // Keep the SQL representation in sync with the in-memory inventory. The web inventory reads
    // this table, so waiting for disconnect or the periodic avatar save would display stale slots.
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      updateDatabase(connection, habbo.getHabboInfo().getId());
    } catch (SQLException e) {
      log.error("Failed to persist inventory for user {}", habbo.getHabboInfo().getId(), e);
    }

    OpenInventoryComposer openInventoryComposer = new OpenInventoryComposer(data.getInventory(),
        Emulator.getConfig().getValue("features.inventory.image.url"), false);

    habbo.getClient().sendResponse(new JavascriptCallbackComposer(openInventoryComposer));
  }


  public boolean hasItem(RPItem item) {
    // Check inventory slots
    for (InventorySlot slot : slots) {
      if (slot.getItem() != null && slot.getItem().equals(item) && slot.isUsable()) {
        return true;
      }
    }

    // Check deposit box
    return depositBox.hasItem(item);
  }

  public int getTotalQuantity(RPItem item) {
    int total = 0;

    // Count from inventory slots (only usable items)
    for (InventorySlot slot : slots) {
      if (slot.getItem() != null && slot.getItem().equals(item) && slot.isUsable()) {
        total += slot.getQuantity();
      }
    }

    // Count from deposit box
    total += depositBox.getTotalQuantity(item);

    return total;
  }

  public RPItem getSlotItem(String name) {
    InventorySlot weaponSlot = getPrimaryWeaponSlot();
    if (isSlot(name, weaponSlot)) {
      return weaponSlot.getItem();
    }

    InventorySlot armorSlot = getSecondaryArmorSlot();
    if (isSlot(name, armorSlot)) {
      return armorSlot.getItem();
    }

    for (int i = REGULAR_SLOTS_START; i < TOTAL_SLOTS; i++) {
      InventorySlot slot = slots[i];
      if (isSlot(name, slot)) {
        return slot.getItem();
      }
    }

    return null;
  }

  public RPItem getSlotItem(Integer itemId) {
    InventorySlot weaponSlot = getPrimaryWeaponSlot();
    if (isSlot(itemId, weaponSlot)) {
      return weaponSlot.getItem();
    }

    InventorySlot armorSlot = getSecondaryArmorSlot();
    if (isSlot(itemId, armorSlot)) {
      return armorSlot.getItem();
    }

    // Check inventory slots (only usable items)
    for (InventorySlot slot : slots) {
      if (isSlot(itemId, slot)) {
        return slot.getItem();
      }
    }

    return null;
  }

  private static boolean isSlot(String name, InventorySlot slot) {
    return !slot.isEmpty() && slot.getItem() != null && slot.getItem().getDisplayName()
        .equalsIgnoreCase(name) && slot.isUsable();
  }

  private static boolean isSlot(Integer itemId, InventorySlot slot) {
    return !slot.isEmpty() && slot.getItem() != null && slot.getItem().getId() == itemId
        && slot.isUsable();
  }

  public InventorySlot getPrimaryWeaponSlot() {
    return slots[PRIMARY_WEAPON_SLOT];
  }

  public InventorySlot getSecondaryArmorSlot() {
    return slots[SECONDARY_ARMOR_SLOT];
  }

  public InventorySlot getSlot(int index) {
    if (index >= 0 && index < TOTAL_SLOTS) {
      return slots[index];
    }
    return null;
  }

  public InventorySlot[] getAllSlots() {
    return Arrays.copyOf(slots, slots.length);
  }

  public boolean equipWeapon(RPItem weapon) {
    if (!"weapon".equals(weapon.getInteractionType())) {
      return false;
    }

    // Check if the weapon is broken in inventory
    InventorySlot sourceSlot = findItemSlot(weapon);
    if (sourceSlot != null && sourceSlot.isBroken()) {
      return false; // Cannot equip broken weapons
    }

    InventorySlot weaponSlot = getPrimaryWeaponSlot();

    // If slot is not empty, move the current weapon to regular inventory or deposit box
    if (!weaponSlot.isEmpty()) {
      unEquipWeapon();
    }

    // Find and remove the weapon from its current slot
    if (sourceSlot != null) {
      RPItem currentWeapon = sourceSlot.getItem();
      int currentQuantity = sourceSlot.getQuantity();
      int currentDurability = sourceSlot.getDurability();

      sourceSlot.removeItem(currentQuantity);

      // Equip the weapon
      weaponSlot.setItem(currentWeapon);
      weaponSlot.setQuantity(currentQuantity);
      weaponSlot.setDurability(currentDurability);
    }

    return true;
  }

  public void unEquipWeapon() {
    InventorySlot weaponSlot = getPrimaryWeaponSlot();

    if (!weaponSlot.isEmpty()) {
      RPItem currentWeapon = weaponSlot.getItem();
      int currentQuantity = weaponSlot.getQuantity();
      int currentDurability = weaponSlot.getDurability();

      weaponSlot.clear();

      // Try to find an empty regular slot first (slots 2-11)
      boolean added = false;
      for (int i = REGULAR_SLOTS_START; i < TOTAL_SLOTS; i++) {
        InventorySlot slot = slots[i];
        if (slot.isEmpty()) {
          slot.setItem(currentWeapon);
          slot.setQuantity(currentQuantity);
          slot.setDurability(currentDurability);
          added = true;
          break;
        }
      }

      // If no empty slot found, add to deposit box
      if (!added) {
        InventorySlot tempSlot = new InventorySlot(false, "regular");
        tempSlot.setItem(currentWeapon);
        tempSlot.setQuantity(currentQuantity);
        tempSlot.setDurability(currentDurability);
        depositBox.getSlots().add(tempSlot);
      }
    }
  }

  public boolean equipArmor(RPItem armor) {
    if (!"shield".equals(armor.getInteractionType())) {
      return false;
    }

    // Check if the armor is broken in inventory
    InventorySlot sourceSlot = findItemSlot(armor);
    if (sourceSlot != null && sourceSlot.isBroken()) {
      return false; // Cannot equip broken armor
    }

    InventorySlot armorSlot = getSecondaryArmorSlot();

    // If slot is not empty, move current armor to regular inventory or deposit box
    if (!armorSlot.isEmpty()) {
      unEquipArmor();
    }

    // Find and remove the armor from its current slot
    if (sourceSlot != null) {
      RPItem currentArmor = sourceSlot.getItem();
      int currentQuantity = sourceSlot.getQuantity();
      int currentDurability = sourceSlot.getDurability();

      sourceSlot.removeItem(currentQuantity);

      // Equip the armor
      armorSlot.setItem(currentArmor);
      armorSlot.setQuantity(currentQuantity);
      armorSlot.setDurability(currentDurability);
    }

    return true;
  }

  public void unEquipArmor() {
    InventorySlot armorSlot = getSecondaryArmorSlot();

    if (!armorSlot.isEmpty()) {
      RPItem currentArmor = armorSlot.getItem();
      int currentQuantity = armorSlot.getQuantity();
      int currentDurability = armorSlot.getDurability();

      armorSlot.clear();

      // Try to find an empty regular slot first
      boolean added = false;
      for (int i = REGULAR_SLOTS_START; i < TOTAL_SLOTS; i++) {
        InventorySlot slot = slots[i];
        if (slot.isEmpty()) {
          slot.setItem(currentArmor);
          slot.setQuantity(currentQuantity);
          slot.setDurability(currentDurability);
          added = true;
          break;
        }
      }

      // If no empty slot found, add to deposit box
      if (!added) {
        InventorySlot tempSlot = new InventorySlot(false, "regular");
        tempSlot.setItem(currentArmor);
        tempSlot.setQuantity(currentQuantity);
        tempSlot.setDurability(currentDurability);
        depositBox.getSlots().add(tempSlot);
      }
    }
  }

  public InventorySlot findItemSlot(RPItem item) {
    InventorySlot weaponSlot = getPrimaryWeaponSlot();
    if (!weaponSlot.isEmpty() && weaponSlot.getItem().equals(item)) {
      return weaponSlot;
    }

    InventorySlot armorSlot = getSecondaryArmorSlot();
    if (!armorSlot.isEmpty() && armorSlot.getItem().equals(item)) {
      return armorSlot;
    }

    for (int i = REGULAR_SLOTS_START; i < TOTAL_SLOTS; i++) {
      InventorySlot slot = slots[i];
      if (!slot.isEmpty() && slot.getItem().equals(item)) {
        return slot;
      }
    }

    return null;
  }

  public void decreaseWeaponDurability(Habbo habbo, int amount) {
    getPrimaryWeaponSlot().decreaseDurability(amount);

    if (getPrimaryWeaponSlot().isBroken()) {
      habbo.getHabboInfo().getCurrentRoom().giveEffect(habbo, 0, -1);
      RPItem item = getPrimaryWeaponSlot().getItem();
      habbo.whisper(
          "*Votre " + item.getDisplayName() + " se brise avec fracas et devient inutilisable*");
      unEquipWeapon();
    }
  }

  public void decreaseArmorDurability(int amount) {
    getSecondaryArmorSlot().decreaseDurability(amount);
  }

  /**
   * Explicitly deletes broken items from inventory (for trash/cleanup commands)
   */
  public List<RPItem> deleteBrokenItems() {
    List<RPItem> deletedItems = new ArrayList<>();

    for (InventorySlot slot : slots) {
      if (slot.isBroken()) {
        deletedItems.add(slot.getItem());
        slot.clear();
      }
    }

    // Also check deposit box
    depositBox.getSlots().removeIf(slot -> {
      if (slot.isBroken()) {
        deletedItems.add(slot.getItem());
        return true;
      }
      return false;
    });

    return deletedItems;
  }

  /**
   * Repair all broken items in inventory (for repair shop commands)
   */
  public List<RPItem> repairAllItems() {
    List<RPItem> repairedItems = new ArrayList<>();

    for (InventorySlot slot : slots) {
      if (slot.isBroken()) {
        slot.repair();
        repairedItems.add(slot.getItem());
      }
    }

    // Also repair items in deposit box
    for (InventorySlot slot : depositBox.getAllSlots()) {
      if (slot.isBroken()) {
        slot.repair();
        repairedItems.add(slot.getItem());
      }
    }

    return repairedItems;
  }


  /**
   * Updates the inventory of the current player in the database using upsert operations. Empty
   * slots are deleted to prevent duplicates with 0 quantity.
   *
   * @param connection The database connection to be used for executing the inventory update
   *                   operations.
   * @throws SQLException If a database access error occurs or the SQL operation fails.
   */
  public synchronized void updateDatabase(Connection connection, int userId) throws SQLException {
    boolean autoCommit = connection.getAutoCommit();
    try {
      connection.setAutoCommit(false);

      try (PreparedStatement deleteStmt = connection.prepareStatement(
          "DELETE FROM user_inventory WHERE user_id = ?")) {
        deleteStmt.setInt(1, userId);
        deleteStmt.executeUpdate();
      }

      try (PreparedStatement insertStmt = connection.prepareStatement(
          "INSERT IGNORE INTO user_inventory (user_id, slot_index, item_id, quantity, durability, is_deposit_box) "
              + "VALUES (?, ?, ?, ?, ?, ?) " + "ON DUPLICATE KEY UPDATE "
              + "item_id = VALUES(item_id), " + "quantity = VALUES(quantity), "
              + "durability = VALUES(durability), " + "is_deposit_box = VALUES(is_deposit_box)")) {

        InventorySlot[] slots = getAllSlots();
        for (int i = 0; i < slots.length; i++) {
          InventorySlot slot = slots[i];

          if (!slot.isEmpty()) {
            insertStmt.setInt(1, userId);
            insertStmt.setInt(2, i);
            insertStmt.setInt(3, slot.getItem().getId());
            insertStmt.setInt(4, slot.getQuantity());
            insertStmt.setInt(5, slot.getDurability());
            insertStmt.setBoolean(6, false);
            insertStmt.addBatch();
          }
        }

        List<InventorySlot> depositSlots = getDepositBox().getAllSlots();
        for (int i = 0; i < depositSlots.size(); i++) {
          InventorySlot slot = depositSlots.get(i);

          if (!slot.isEmpty()) {
            insertStmt.setInt(1, userId);
            insertStmt.setInt(2, i);
            insertStmt.setInt(3, slot.getItem().getId());
            insertStmt.setInt(4, slot.getQuantity());
            insertStmt.setInt(5, slot.getDurability());
            insertStmt.setBoolean(6, true);
            insertStmt.addBatch();
          }
        }

        insertStmt.executeBatch();
      }

      connection.commit();
    } catch (SQLException e) {
      connection.rollback();
      throw e;
    } finally {
      connection.setAutoCommit(autoCommit);
    }
  }

  /**
   * Explicitly deletes broken/empty inventory records from the database. This method should only be
   * called when explicitly cleaning up the database.
   */
  public void cleanupInventoryDatabase(int userId) {
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement deleteStmt = connection.prepareStatement(
        "DELETE FROM user_inventory WHERE user_id = ? AND (quantity <= 0 OR durability <= 0)")) {

      deleteStmt.setInt(1, userId);
      int deletedRecords = deleteStmt.executeUpdate();

      log.debug("Cleaned up {} broken inventory records for user {}", deletedRecords, userId);
    } catch (SQLException e) {
      log.error("Failed to cleanup inventory database for user {}", userId, e);
    }
  }

  @Override
  public String toString() {
    StringBuilder sb = new StringBuilder();
    sb.append("=== INVENTORY ===\n");

    for (int i = 0; i < slots.length; i++) {
      InventorySlot slot = slots[i];
      sb.append("Slot ").append(i + 1);

      if (i == PRIMARY_WEAPON_SLOT) {
        sb.append(" (Primary Weapon)");
      } else if (i == SECONDARY_ARMOR_SLOT) {
        sb.append(" (Secondary Armor)");
      }

      sb.append(": ");

      if (slot.isEmpty()) {
        sb.append("Empty");
      } else {
        sb.append(slot.getQuantity()).append(" ").append(slot.getItem().getDisplayName());
        if (slot.isDurabilityItem()) {
          sb.append(" (").append(String.format("%.0f", slot.getDurabilityPercentage() * 100))
              .append("% durability)");
          if (slot.isBroken()) {
            sb.append(" [BROKEN]");
          }
        }
      }
      sb.append("\n");
    }

    sb.append("\n=== DEPOSIT BOX ===\n");
    if (depositBox.getSlotCount() == 0) {
      sb.append("Empty\n");
    } else {
      for (int i = 0; i < depositBox.getAllSlots().size(); i++) {
        InventorySlot slot = depositBox.getAllSlots().get(i);
        sb.append("Deposit Slot ").append(i + 1).append(": ");
        sb.append(slot.getQuantity()).append(" ").append(slot.getItem().getDisplayName());
        if (slot.isBroken()) {
          sb.append(" [BROKEN]");
        }
        sb.append("\n");
      }
    }

    return sb.toString();
  }
}
