package io.github.brenoepics.roleplay.features.macro;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import java.util.Objects;
import org.json.JSONObject;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class MacroManager {

  public static final Logger LOGGER = LoggerFactory.getLogger(MacroManager.class);

  private static HashMap<Integer, HashMap<Integer, Macro>> userMacros;
  public int chatLimit;

  public MacroManager() {
    userMacros = new HashMap<>();
    this.chatLimit = Emulator.getConfig().getInt("hotel.chat.max.length", 100);
  }

  public Map<Integer, Macro> loadUser(Habbo habbo) {
    userMacros.remove(habbo.getHabboInfo().getId());

    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "SELECT * FROM users_macros WHERE user_id = ?")) {
      statement.setInt(1, habbo.getHabboInfo().getId());
      try (ResultSet set = statement.executeQuery()) {
        while (set.next()) {
          Macro macro = new Macro(set.getInt("id"), set.getString("name"),
              set.getString("configs"));
          if (userMacros.containsKey(habbo.getHabboInfo().getId())) {
            userMacros.get(habbo.getHabboInfo().getId()).put(macro.getId(), macro);
            continue;
          }

          userMacros.put(habbo.getHabboInfo().getId(),
              new HashMap<>(Collections.singletonMap(macro.getId(), macro)));

        }
      }
    } catch (SQLException e) {
      LOGGER.error("Caught SQL exception", e);
    }
    return userMacros.getOrDefault(habbo.getHabboInfo().getId(), null);
  }


  public Map<Integer, Macro> getUserMacros(int userId) {
    return userMacros.getOrDefault(userId, new HashMap<>());
  }

  public void addMacro(int userId, Macro macro) {
    if (userMacros.containsKey(userId)) {
      userMacros.get(userId).put(macro.getId(), macro);
    } else {
      userMacros.put(userId, new HashMap<>(Collections.singletonMap(macro.getId(), macro)));
    }
  }

  public boolean createMacro(int userId, String name) {
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "INSERT INTO users_macros (user_id, name, configs) VALUES (?, ?, ?)",
        Statement.RETURN_GENERATED_KEYS)) {
      statement.setInt(1, userId);
      statement.setString(2, name);
      statement.setString(3, "{}");
      statement.execute();
      try (ResultSet set = statement.getGeneratedKeys()) {
        if (set.next()) {
          int id = set.getInt(1);
          this.addMacro(userId, new Macro(id, name, new HashMap<>()));
          return true;
        }
      }
    } catch (SQLException e) {
      LOGGER.error("Caught SQL exception", e);
    }
    return false;
  }

  public boolean editMacro(int userId, int macroId, JSONObject macroSet) {
    Macro macro = this.getUserMacro(userId, macroId);
    if (macro == null) {
      return false;
    }
    macro.reload(macroSet);
    return true;
  }

  public boolean deleteMacro(int userId, int macroId) {
    Macro macro = this.getUserMacro(userId, macroId);
    if (macro == null) {
      return false;
    }
    Map<Integer, Macro> macros = this.getUserMacros(userId);
    if (macros == null || macros.isEmpty()) {
      return true;
    }
    macro.delete();
    userMacros.get(userId).remove(macro.getId());
    return true;
  }

  public Macro getUserMacro(int userId, String macro) {
    Map<Integer, Macro> macros = this.getUserMacros(userId);
    if (macros == null || macros.isEmpty()) {
      return null;
    }
    return macros.values().stream().filter(m -> m.getName().equals(macro)).findAny().orElse(null);
  }

  public Macro getUserMacro(int userId, int macroId) {
    Map<Integer, Macro> macros = this.getUserMacros(userId);
    if (macros == null || macros.isEmpty()) {
      return null;
    }
    return macros.values().stream().filter(m -> m.getId() == macroId).findAny().orElse(null);
  }

  public boolean habboHasMacro(int userId, String macro) {
    Map<Integer, Macro> macros = this.getUserMacros(userId);
    if (macros == null || macros.isEmpty()) {
      return false;
    }
    return macros.values().stream().anyMatch(m -> Objects.equals(m.getName(), macro));
  }

  public boolean habboHasMacro(int userId, int macro) {
    Map<Integer, Macro> macros = this.getUserMacros(userId);
    if (macros == null || macros.isEmpty()) {
      return false;
    }
    return macros.values().stream().anyMatch(m -> m.getId() == macro);
  }
}
