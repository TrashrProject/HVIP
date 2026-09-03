package io.github.brenoepics.roleplay.utilities.types;

import java.util.Locale;
import java.util.concurrent.ThreadLocalRandom;

/**
 * Small parser for weapon combat data stored in rp_items.extra_data.
 *
 * Supported profile format:
 * mode=ranged;damage=10-15;range=6;durability=1;magazine=5
 *
 * Legacy numeric values ("3" or "2,5") are still accepted for melee items.
 */
public final class WeaponProfile {

  private static final int DEFAULT_RANGED_MIN_DAMAGE = 25;
  private static final int DEFAULT_RANGED_MAX_DAMAGE = 35;
  private static final int DEFAULT_RANGED_RANGE = 3;
  private static final int DEFAULT_MELEE_MIN_DAMAGE = 1;
  private static final int DEFAULT_MELEE_MAX_DAMAGE = 3;

  private final boolean ranged;
  private final int minDamage;
  private final int maxDamage;
  private final int range;
  private final int durabilityLoss;
  private final int magazineSize;

  private WeaponProfile(boolean ranged, int minDamage, int maxDamage, int range,
      int durabilityLoss, int magazineSize) {
    this.ranged = ranged;
    this.minDamage = Math.max(0, minDamage);
    this.maxDamage = Math.max(this.minDamage, maxDamage);
    this.range = Math.max(1, Math.min(20, range));
    this.durabilityLoss = Math.max(0, Math.min(100, durabilityLoss));
    this.magazineSize = Math.max(0, Math.min(1000, magazineSize));
  }

  public static WeaponProfile from(RPItem item) {
    String name = item == null || item.getDisplayName() == null ? "" : item.getDisplayName();
    String normalizedName = name.toLowerCase(Locale.ROOT).replaceAll("[^a-z0-9]", "");
    boolean inferredRanged = isKnownRangedName(normalizedName)
        || (item != null && item.getEnableId() == 164);

    int minDamage = inferredRanged ? DEFAULT_RANGED_MIN_DAMAGE : DEFAULT_MELEE_MIN_DAMAGE;
    int maxDamage = inferredRanged ? DEFAULT_RANGED_MAX_DAMAGE : DEFAULT_MELEE_MAX_DAMAGE;
    int range = inferredRanged ? DEFAULT_RANGED_RANGE : 1;
    int durabilityLoss = 1;
    int magazineSize = 0;
    boolean ranged = inferredRanged;

    String raw = item == null ? null : item.getExtraData();
    if (raw == null || raw.isBlank()) {
      return new WeaponProfile(ranged, minDamage, maxDamage, range, durabilityLoss, magazineSize);
    }

    String data = raw.trim();
    if (!data.contains("=")) {
      int[] legacyDamage = parseDamage(data, minDamage, maxDamage);
      return new WeaponProfile(ranged, legacyDamage[0], legacyDamage[1], range, durabilityLoss,
          magazineSize);
    }

    for (String token : data.split(";")) {
      String[] pair = token.split("=", 2);
      if (pair.length != 2) {
        continue;
      }

      String key = pair[0].trim().toLowerCase(Locale.ROOT);
      String value = pair[1].trim();
      switch (key) {
        case "mode", "type" -> ranged = value.equalsIgnoreCase("ranged")
            || value.equalsIgnoreCase("gun") || value.equalsIgnoreCase("firearm");
        case "damage", "dmg" -> {
          int[] damage = parseDamage(value, minDamage, maxDamage);
          minDamage = damage[0];
          maxDamage = damage[1];
        }
        case "range", "distance" -> range = parseInt(value, range);
        case "durability", "durability_loss", "wear" -> durabilityLoss = parseInt(value,
            durabilityLoss);
        case "magazine", "clip", "ammo" -> magazineSize = parseInt(value, magazineSize);
        default -> {
          // Forward compatible: unknown profile keys are intentionally ignored.
        }
      }
    }

    return new WeaponProfile(ranged, minDamage, maxDamage, range, durabilityLoss, magazineSize);
  }

  private static boolean isKnownRangedName(String value) {
    return value.contains("pistol") || value.contains("usp") || value.contains("ak47")
        || value.contains("akm") || value.contains("g36") || value.contains("mp5")
        || value.contains("sniper") || value.contains("rifle") || value.contains("gun");
  }

  private static int[] parseDamage(String value, int fallbackMin, int fallbackMax) {
    if (value == null || value.isBlank()) {
      return new int[]{fallbackMin, fallbackMax};
    }

    String normalized = value.trim().replace("..", "-");
    String separator = normalized.contains("-") ? "-" : (normalized.contains(",") ? "," : null);
    try {
      if (separator == null) {
        int damage = Integer.parseInt(normalized);
        return new int[]{damage, damage};
      }

      String[] parts = normalized.split(java.util.regex.Pattern.quote(separator), 2);
      int min = Integer.parseInt(parts[0].trim());
      int max = Integer.parseInt(parts[1].trim());
      if (min > max) {
        int swap = min;
        min = max;
        max = swap;
      }
      return new int[]{min, max};
    } catch (NumberFormatException exception) {
      return new int[]{fallbackMin, fallbackMax};
    }
  }

  private static int parseInt(String value, int fallback) {
    try {
      return Integer.parseInt(value.trim());
    } catch (Exception exception) {
      return fallback;
    }
  }

  public int rollDamage() {
    if (minDamage == maxDamage) {
      return minDamage;
    }
    return ThreadLocalRandom.current().nextInt(minDamage, maxDamage + 1);
  }

  public boolean isRanged() {
    return ranged;
  }

  public int getMinDamage() {
    return minDamage;
  }

  public int getMaxDamage() {
    return maxDamage;
  }

  public int getRange() {
    return range;
  }

  public int getDurabilityLoss() {
    return durabilityLoss;
  }

  public int getMagazineSize() {
    return magazineSize;
  }
}
