package io.github.brenoepics.roleplay.features.skins;

import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import lombok.extern.slf4j.Slf4j;

/** Resolves the visual effect selected by a player for an equipped RP weapon. */
@Slf4j
public class WeaponSkinService {

  public int getEquippedEffect(int userId, String weaponName, int fallbackEffect) {
    String key = normalizeWeaponKey(weaponName);
    if (key == null) return fallbackEffect;

    String sql = "SELECT s.effect_id FROM paradise_user_weapon_skins us "
        + "INNER JOIN paradise_weapon_skins s ON s.id=us.skin_id "
        + "WHERE us.user_id=? AND us.equipped=1 AND s.weapon_key=? LIMIT 1";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
         PreparedStatement statement = connection.prepareStatement(sql)) {
      statement.setInt(1, userId);
      statement.setString(2, key);
      try (ResultSet result = statement.executeQuery()) {
        if (result.next()) return result.getInt("effect_id");
      }
    } catch (Exception exception) {
      // A missing migration must not prevent weapons from being equipped.
      log.warn("Unable to resolve weapon skin for user {} and weapon {}", userId, key,
          exception);
    }
    return fallbackEffect;
  }

  private String normalizeWeaponKey(String weaponName) {
    if (weaponName == null) return null;
    String value = weaponName.toLowerCase().replaceAll("[^a-z0-9]", "");
    if (value.contains("tazor") || value.contains("taser") || value.contains("stun")) return "tazor";
    if (value.contains("ak47")) return "ak47";
    if (value.contains("akm")) return "akm";
    if (value.contains("g36")) return "g36";
    return null;
  }
}
