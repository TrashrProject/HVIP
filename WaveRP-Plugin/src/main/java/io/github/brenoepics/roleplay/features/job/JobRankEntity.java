
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
    return getPermissions().contains(permission);
  }

  public boolean hasAnyPermission(String... permissions) {
    Set<String> rankPermissions = getPermissions();
    for (String permission : permissions) {
      if (rankPermissions.contains(permission)) {
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