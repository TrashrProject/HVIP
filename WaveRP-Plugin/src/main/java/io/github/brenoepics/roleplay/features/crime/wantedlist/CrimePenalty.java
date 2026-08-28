package io.github.brenoepics.roleplay.features.crime.wantedlist;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class CrimePenalty {
  private int starLevel;
  private int jailTimeMinutes;
  private int fineAmount;

  public String getDisplayStars() {
    return "⭐".repeat(Math.max(0, starLevel));
  }
}