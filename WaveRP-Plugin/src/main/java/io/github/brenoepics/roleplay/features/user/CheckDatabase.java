package io.github.brenoepics.roleplay.features.user;

import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class CheckDatabase {

  private static final Logger LOGGER = LoggerFactory.getLogger(CheckDatabase.class);

  private static final String PERMISSION_TABLE = "permissions";

  public enum PermissionResult {
    REGISTERED, ALREADY_REGISTERED, ERROR
  }

  public enum PermissionState {
    DENIED(0), ALLOWED(1), ROOM_OWNER(2);

    public final int stateId;

    PermissionState(int i) {
      this.stateId = i;
    }
  }

  public static void registerPermission(String name, PermissionState defaultValue) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      try (PreparedStatement statement = connection.prepareStatement(
          "SELECT `column_name` FROM INFORMATION_SCHEMA.COLUMNS WHERE `table_name` = ? AND `column_name` = ?")) {
        statement.setString(1, PERMISSION_TABLE);
        statement.setString(2, name);
        if (!statement.executeQuery().next()) {
          try (PreparedStatement stmt = connection.prepareStatement(
              "ALTER TABLE `" + PERMISSION_TABLE + "` ADD `" + name
                  + "` ENUM('0', '1', '2') NOT NULL DEFAULT ?")) {
            stmt.setString(1, defaultValue.stateId + "");
            stmt.execute();
          }
        }
      }
    } catch (SQLException sql) {
      LOGGER.error("[NaHabbo RolePlay] Failed to check database for permission: {}", name, sql);
    }
  }

  public static void allowPermissionForRankRange(String name, int minimumRank, int maximumRank) {
    if (!name.matches("[A-Za-z0-9_]+")) {
      throw new IllegalArgumentException("Invalid permission name");
    }

    String query = "UPDATE `" + PERMISSION_TABLE + "` SET `" + name
        + "` = CASE WHEN `id` BETWEEN ? AND ? THEN '1' ELSE '0' END";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(query)) {
      statement.setInt(1, minimumRank);
      statement.setInt(2, maximumRank);
      statement.executeUpdate();
    } catch (SQLException sql) {
      LOGGER.error("[NaHabbo RolePlay] Failed to configure permission: {}", name, sql);
    }
  }
}
