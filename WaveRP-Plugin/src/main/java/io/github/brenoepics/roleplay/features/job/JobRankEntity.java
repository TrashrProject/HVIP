
package io.github.brenoepics.roleplay.features.job;

import java.util.HashSet;
import java.util.Set;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
import lombok.EqualsAndHashCode;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.core.type.TypeReference;
import lombok.extern.slf4j.Slf4j;

@Slf4j
@Data
@NoArgsConstructor
@AllArgsConstructor
@EqualsAndHashCode(exclude = {"job", "parsedPermissions"}) // Exclude circular reference fields
public class JobRankEntity {
  private int id;
  private int jobId;
  private String name;
  private String displayName;
  private int level;
  private boolean isManager;
  private BigDecimal salary;
  private String permissions; // JSON string
  private boolean active;
  private LocalDateTime createdAt;
  private LocalDateTime updatedAt;
  private JobEntity job;
  private Set<String> parsedPermissions;
  private static final ObjectMapper objectMapper = new ObjectMapper();

  public boolean isHigherOrEqualThan(JobRankEntity other) {
    return this.level >= other.getLevel();
  }

  public Set<String> getPermissions() {
    if (parsedPermissions == null) {
      parsePermissions();
    }
    return parsedPermissions;
  }

  private void parsePermissions() {
    if (permissions == null || permissions.trim().isEmpty()) {
      parsedPermissions = new HashSet<>();
      return;
    }

    try {
      parsedPermissions = objectMapper.readValue(permissions, new TypeReference<Set<String>>() {});
    } catch (Exception e) {
      log.warn("Failed to parse permissions for rank {}: {}", name, permissions, e);
      parsedPermissions = new HashSet<>();
    }
  }

  public boolean hasPermission(String permission) {
    Set<String> rankPermissions = getPermissions();
    if (rankPermissions.contains(permission)) {
      return true;
    }

    // Compatibilité avec les permissions restaurant déjà créées dans ParadiseRP.
    // Les premiers grades Zy'Croque / Tasty Crousty ont été enregistrés avec les noms
    // de commandes (menu, prendrecommande, preparer...) alors que le moteur utilise
    // maintenant des permissions namespacées restaurant.*. On accepte les deux formats
    // afin que les anciens grades fonctionnent sans casser les autres métiers.
    String legacyRestaurantPermission = switch (permission) {
      case JobPermissions.RESTAURANT_MENU -> "menu";
      case JobPermissions.RESTAURANT_ORDER -> "prendrecommande";
      case JobPermissions.RESTAURANT_PREPARE -> "preparer";
      case JobPermissions.RESTAURANT_SERVE -> "servir";
      case JobPermissions.RESTAURANT_BILL -> "addition";
      case JobPermissions.RESTAURANT_CASH -> "encaisser";
      case JobPermissions.RESTAURANT_KITCHEN -> "kitchen";
      default -> null;
    };

    return legacyRestaurantPermission != null && rankPermissions.contains(legacyRestaurantPermission);
  }

  public boolean hasAnyPermission(String... permissions) {
    for (String permission : permissions) {
      if (hasPermission(permission)) {
        return true;
      }
    }
    return false;
  }

  @Override
  public String toString() {
    return displayName;
  }
}