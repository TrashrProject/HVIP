package io.github.brenoepics.roleplay.utilities.template;


import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring an attacker and victim (combat). Methods enforced: format(LivePlayer
 * attacker, LivePlayer victim) or format(String attackerName, String victimName)
 */
public enum CombatTemplates {
  KILL("%attacker% a mis %victim% K.-O."), HIT(
      "%attacker% a frapp\u00e9 %victim% au visage"), SHOOT(
      "%attacker% a tir\u00e9 sur %victim%"), TAZOR(
      "%attacker% a tas\u00e9 %victim%"), ROB(
      "%attacker% a d\u00e9rob\u00e9 les biens de %victim%"), ARREST(
      "%attacker% a arr\u00eat\u00e9 %victim% pour %action%"),
  AUTO_MURDER("%attacker% a commis un homicide sur %victim%"), AUTO_GANG_HOMICIDE(
      "%attacker% a abattu le membre d'un gang rival, %victim%"), AUTO_MASS_MURDER(
      "%attacker% a ajout\u00e9 %victim% \u00e0 sa liste de victimes"), AUTO_COP_MURDER(
      "%attacker% a mis K.-O. l'agent en service %victim%"), AUTO_EXECUTION(
      "%attacker% a ex\u00e9cut\u00e9 le prisonnier %victim%"),
  OFFICER_CHARGED("%attacker% a inculp\u00e9 %victim% pour %crime%"), CRIMINAL_ALERT(
      "[INCULPATION AUTOMATIQUE] %attacker% a commis : %crime%"), NOTIFY_CRIMINAL(
      "L'agent %attacker% vous a inculp\u00e9 pour : %crime%"), NOTIFY_OFFICER(
      "Vous avez inculp\u00e9 %victim% pour : %crime%"), AUTO_CRIME_NOTIFY(
      "[INCULPATION AUTOMATIQUE] Vous avez commis : %crime%");


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
