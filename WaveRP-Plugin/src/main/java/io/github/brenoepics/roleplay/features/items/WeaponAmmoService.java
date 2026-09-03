package io.github.brenoepics.roleplay.features.items;

import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public final class WeaponAmmoService {

  public static final int ERROR = -2;
  public static final int EMPTY = -1;

  private WeaponAmmoService() {
  }

  public static int getAmmo(int userId, int itemId, int magazineSize) {
    if (magazineSize <= 0) {
      return 0;
    }

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      ensureMagazine(connection, userId, itemId, magazineSize);
      try (PreparedStatement statement = connection.prepareStatement(
          "SELECT current_ammo FROM paradise_weapon_ammo WHERE user_id = ? AND item_id = ?")) {
        statement.setInt(1, userId);
        statement.setInt(2, itemId);
        try (ResultSet result = statement.executeQuery()) {
          return result.next() ? result.getInt("current_ammo") : magazineSize;
        }
      }
    } catch (Exception exception) {
      log.error("Unable to read ammo for user {} item {}", userId, itemId, exception);
      return ERROR;
    }
  }

  public static int consumeRound(int userId, int itemId, int magazineSize) {
    if (magazineSize <= 0) {
      return 0;
    }

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      connection.setAutoCommit(false);
      try {
        ensureMagazine(connection, userId, itemId, magazineSize);
        int current;
        try (PreparedStatement select = connection.prepareStatement(
            "SELECT current_ammo FROM paradise_weapon_ammo "
                + "WHERE user_id = ? AND item_id = ? FOR UPDATE")) {
          select.setInt(1, userId);
          select.setInt(2, itemId);
          try (ResultSet result = select.executeQuery()) {
            if (!result.next()) {
              connection.rollback();
              return ERROR;
            }
            current = result.getInt("current_ammo");
          }
        }

        if (current <= 0) {
          connection.rollback();
          return EMPTY;
        }

        int remaining = current - 1;
        try (PreparedStatement update = connection.prepareStatement(
            "UPDATE paradise_weapon_ammo SET current_ammo = ?, updated_at = CURRENT_TIMESTAMP "
                + "WHERE user_id = ? AND item_id = ?")) {
          update.setInt(1, remaining);
          update.setInt(2, userId);
          update.setInt(3, itemId);
          update.executeUpdate();
        }
        connection.commit();
        return remaining;
      } catch (Exception exception) {
        connection.rollback();
        throw exception;
      } finally {
        connection.setAutoCommit(true);
      }
    } catch (Exception exception) {
      log.error("Unable to consume ammo for user {} item {}", userId, itemId, exception);
      return ERROR;
    }
  }

  public static int addAmmo(int userId, int itemId, int magazineSize, int rounds) {
    if (magazineSize <= 0 || rounds <= 0) {
      return getAmmo(userId, itemId, magazineSize);
    }

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      connection.setAutoCommit(false);
      try {
        ensureMagazine(connection, userId, itemId, magazineSize);
        int current;
        try (PreparedStatement select = connection.prepareStatement(
            "SELECT current_ammo FROM paradise_weapon_ammo "
                + "WHERE user_id = ? AND item_id = ? FOR UPDATE")) {
          select.setInt(1, userId);
          select.setInt(2, itemId);
          try (ResultSet result = select.executeQuery()) {
            if (!result.next()) {
              connection.rollback();
              return ERROR;
            }
            current = result.getInt("current_ammo");
          }
        }

        int updated = Math.min(magazineSize, current + rounds);
        try (PreparedStatement update = connection.prepareStatement(
            "UPDATE paradise_weapon_ammo SET current_ammo = ?, updated_at = CURRENT_TIMESTAMP "
                + "WHERE user_id = ? AND item_id = ?")) {
          update.setInt(1, updated);
          update.setInt(2, userId);
          update.setInt(3, itemId);
          update.executeUpdate();
        }
        connection.commit();
        return updated;
      } catch (Exception exception) {
        connection.rollback();
        throw exception;
      } finally {
        connection.setAutoCommit(true);
      }
    } catch (Exception exception) {
      log.error("Unable to reload ammo for user {} item {}", userId, itemId, exception);
      return ERROR;
    }
  }

  private static void ensureMagazine(Connection connection, int userId, int itemId, int magazineSize)
      throws Exception {
    try (PreparedStatement insert = connection.prepareStatement(
        "INSERT IGNORE INTO paradise_weapon_ammo (user_id, item_id, current_ammo) VALUES (?, ?, ?)")) {
      insert.setInt(1, userId);
      insert.setInt(2, itemId);
      insert.setInt(3, magazineSize);
      insert.executeUpdate();
    }
  }
}
