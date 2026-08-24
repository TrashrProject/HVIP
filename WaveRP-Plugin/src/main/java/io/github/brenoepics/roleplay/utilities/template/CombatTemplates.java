package io.github.brenoepics.roleplay.utilities.template;


import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring an attacker and victim (combat). Methods enforced: format(LivePlayer
 * attacker, LivePlayer victim) or format(String attackerName, String victimName)
 */
public enum CombatTemplates {
  KILL("%attacker% knocked out %victim%"), HIT(
      "%attacker% smacked %victim% right in the face"), SHOOT(
      "%attacker% stunned %victim%"), TAZOR(
      "%attacker% robbed %victim%"), ROB(
      "%attacker% relieved %victim% of their valuables"), ARREST(
      "%attacker% arrested %victim% for %action%"), // Auto-charge-related templates
  AUTO_MURDER("%attacker% committed homicide against %victim%"), AUTO_GANG_HOMICIDE(
      "%attacker% executed a rival gang member, %victim%"), AUTO_MASS_MURDER(
      "%attacker% added %victim% to their growing list of victims"), AUTO_COP_MURDER(
      "%attacker% knocked out on-duty cop %victim%"), AUTO_EXECUTION(
      "%attacker% carried out the execution of prisoner %victim%"), // Police and crime notification templates
  OFFICER_CHARGED("%attacker% charged %victim% with %crime%"), CRIMINAL_ALERT(
      "[AUTO CHARGE] %attacker% committed %crime%"), NOTIFY_CRIMINAL(
      "You've been charged with: %crime% by officer %attacker%"), NOTIFY_OFFICER(
      " You've charged %victim% with: %crime%"), AUTO_CRIME_NOTIFY(
      "[AUTO CHARGE] You've committed a crime: %crime%");


  private final String template;

  CombatTemplates(String template) {
    this.template = template;
  }

  /**
   * Format this template with an attacker and victim.
   *
   * @param attacker The player initiating the action
   * @param victim   The player receiving the action
   * @return The formatted message
   */
  public String format(LivePlayer attacker, LivePlayer victim) {
    return LiveFeed.formatCombat(template, attacker, victim);
  }

  /**
   * Format this template with attacker and victim names. Uses default player state of NORMAL for
   * both.
   *
   * @param attackerName The name of the player initiating the action
   * @param victimName   The name of the player receiving the action
   * @return The formatted message
   */
  public String format(String attackerName, String victimName) {
    return format(new LivePlayer(attackerName, PlayerState.ATTACKER),
        new LivePlayer(victimName, PlayerState.VICTIM));
  }

  /**
   * Format this template with attacker and victim info.
   *
   * @param attackerName  The name of the player initiating the action
   * @param attackerState The state of the attacker
   * @param victimName    The name of the player receiving the action
   * @param victimState   The state of the victim
   * @return The formatted message
   */
  public String format(String attackerName, PlayerState attackerState, String victimName,
      PlayerState victimState) {
    return format(new LivePlayer(attackerName, attackerState),
        new LivePlayer(victimName, victimState));
  }

  public String format(String attackerName, String victimName, String arg) {
    return format(new LivePlayer(attackerName, PlayerState.ATTACKER),
        new LivePlayer(victimName, PlayerState.VICTIM)).replace("%action%", arg);
  }
}
