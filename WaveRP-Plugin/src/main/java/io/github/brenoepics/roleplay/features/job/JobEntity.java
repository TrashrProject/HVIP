package io.github.brenoepics.roleplay.features.job;

import java.time.LocalDateTime;
import java.util.List;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.EqualsAndHashCode;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
@EqualsAndHashCode(exclude = {"ranks"})
public class JobEntity {
  private int id;
  private String name;
  private String displayName;
  private String description;
  private boolean active;
  private LocalDateTime createdAt;
  private LocalDateTime updatedAt;
  private List<JobRankEntity> ranks;

  public JobEntity(int id, String name, String displayName, String description, boolean active) {
    this.id = id;
    this.name = name;
    this.displayName = displayName;
    this.description = description;
    this.active = active;
  }

  public boolean isUnemployed() {
    return "unemployed".equals(name);
  }

  @Override
  public String toString() {
    return displayName;
  }
}