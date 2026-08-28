package io.github.brenoepics.roleplay.features.macro;

import com.eu.habbo.Emulator;
import io.github.brenoepics.roleplay.RolePlay;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.util.HashMap;
import java.util.Map;
import lombok.Getter;
import lombok.extern.slf4j.Slf4j;
import org.json.JSONObject;

@Slf4j
@Getter
public class Macro {
  private final int id;
  private String name;
  private final HashMap<String, String> texts;

  public Macro(int id, String name, Map<String, String> macros) {
    this.id = id;
    this.name = name;
    this.texts = new HashMap<>(macros);
  }

  public Macro(int id, String name, String macrosSet) {
    this.id = id;
    this.name = name;
    this.texts = new HashMap<>();

    JSONObject jsonObject = new JSONObject(macrosSet);
    if (jsonObject.isEmpty()) return;

    jsonObject
        .keys()
        .forEachRemaining(
            key -> {
              Object value = jsonObject.get(key);
              this.texts.put(key, value.toString());
            });
  }

  private void update() {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      try (PreparedStatement statement =
          connection.prepareStatement(
              "UPDATE `users_macros` SET `configs` = ?, `name` = ? WHERE `id` = ? LIMIT 1")) {
        if (this.texts.isEmpty()) {
          statement.setString(1, "{}");
        } else statement.setString(1, new JSONObject(this.texts).toString());
        statement.setString(2, this.name);
        statement.setInt(3, this.id);
        statement.executeUpdate();
      }
    } catch (SQLException e) {
      log.error("Error: ", e);
    }
  }

  public void reload(String macroSet) {
    this.reload(new JSONObject(macroSet));
  }

  public void reload(JSONObject jsonObject) {
    this.texts.clear();
    if (jsonObject == null || jsonObject.isEmpty()) return;

    jsonObject
        .keys()
        .forEachRemaining(
            key -> {
              Object value = jsonObject.get(key);
              if (key.length() < 20
                  && value.toString().length() < RolePlay.getMacroManager().chatLimit) {
                this.texts.put(key, value.toString());
              }
            });
    this.update();
  }

  public boolean delete() {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement =
            connection.prepareStatement("DELETE FROM users_macros WHERE id = ? LIMIT 1")) {
      statement.setInt(1, this.getId());
      return statement.execute();
    } catch (SQLException e) {
      MacroManager.LOGGER.error("Caught SQL exception", e);
    }
    return false;
  }

  public void setName(String name) {
    this.name = name;
    this.update();
  }
}
