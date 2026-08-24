package io.github.brenoepics.roleplay.features.user;

import lombok.Getter;
import lombok.Setter;

/**
 * Class representing combat-related statistics for an RPAvatar.
 */
@Getter
@Setter
public class CombatStats {
  private int kills;
  private int deaths;
  private int arrests;
  private double kdRatio;
  private int punchesThrown;
  private int punchesReceived;
  private int damageDealt;
  private int damageReceived;

  /**
   * Constructs a new CombatStats instance with default values.
   */
  public CombatStats() {
    this.kills = 0;
    this.deaths = 0;
    this.arrests = 0;
    this.kdRatio = 0.0;
    this.punchesThrown = 0;
    this.punchesReceived = 0;
    this.damageDealt = 0;
    this.damageReceived = 0;
  }

  /**
   * Updates the K/D ratio based on kills and deaths.
   */
  public void updateKdRatio() {
    if (deaths > 0) {
      this.kdRatio = (double) kills / deaths;
    } else {
      this.kdRatio = kills; // If no deaths, K/D ratio equals kills
    }
  }

  /**
   * Records a kill, increments the kill counter and updates the K/D ratio.
   */
  public void recordKill() {
    this.kills++;
    updateKdRatio();
  }

  /**
   * Records a death, increments the death counter and updates the K/D ratio.
   */
  public void recordDeath() {
    this.deaths++;
    updateKdRatio();
  }

  /**
   * Records damage dealt to another avatar.
   *
   * @param amount The amount of damage dealt
   */
  public void recordDamageDealt(int amount) {
    this.damageDealt += amount;
  }

  /**
   * Records damage received from another avatar.
   *
   * @param amount The amount of damage received
   */
  public void recordDamageReceived(int amount) {
    this.damageReceived += amount;
  }

  /**
   * Records a punch thrown.
   */
  public void recordPunchThrown() {
    this.punchesThrown++;
  }

  /**
   * Records a punch received.
   */
  public void recordPunchReceived() {
    this.punchesReceived++;
  }

  /**
   * Records an arrest (for police/law enforcement roles).
   */
  public void recordArrest() {
    this.arrests++;
  }
}