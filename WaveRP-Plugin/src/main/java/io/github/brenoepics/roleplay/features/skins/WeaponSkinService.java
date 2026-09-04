package io.github.brenoepics.roleplay.features.skins;

import com.eu.habbo.Emulator;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import lombok.extern.slf4j.Slf4j;

/** Resolves the visual effect selected by a player for an equipped RP weapon. */
@Slf4j
public class WeaponSkinService {

  /**
   * Temporary safety gate for ParadiseRP weapon skins.
   *
   * The non-default skin effect ids currently configured in SQL are not all backed by working
   * Nitro effects on the live client. Returning one of those ids makes the equipped weapon appear
   * to vanish even though it is still present in inventory slot 0. Until each custom effect is
   * validated/imported, always keep the weapon's native RP enable id as the live visual effect.
   *
   * Skin ownership/selection stays persisted in the database, so the UI work is not lost. Once the
   * missing Nitro effects are installed, this guard can be relaxed per validated effect.
   */
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
        if (result.next()) {
          int selectedEffect = result.getInt("effect_id");
          if (selectedEffect != fallbackEffect) {
            log.debug(
                "Skin effect {} selected for user {} weapon {}, using stable fallback {} until the Nitro effect is validated",
                selectedEffect, userId, key, fallbackEffect);
          }
        }
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
