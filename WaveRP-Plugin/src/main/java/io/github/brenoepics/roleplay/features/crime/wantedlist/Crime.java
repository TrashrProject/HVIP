package io.github.brenoepics.roleplay.features.crime.wantedlist;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class Crime {

  private int id;
  private String name;
  private int stars;
  private boolean policeAlert;
  private boolean instantAlert;
  private boolean autoCharge;
  private String notes;

  public String getDisplayStars() {
    return "⭐".repeat(Math.max(0, stars));
  }

  public boolean isManualCharge() {
    return !autoCharge;
  }
}