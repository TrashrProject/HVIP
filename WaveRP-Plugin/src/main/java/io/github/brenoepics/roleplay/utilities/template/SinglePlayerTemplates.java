package io.github.brenoepics.roleplay.utilities.template;

import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring a single player. Methods enforced: format(String playerName) or
 * format(LivePlayer player)
 */
public enum SinglePlayerTemplates {
  START_WORK("%player% a commenc\u00e9 son service"),
  STOP_WORK("%player% a termin\u00e9 son service"),
  HELP("%player% a demand\u00e9 des renforts"),
  PASSIVE_ON("%player% est pass\u00e9 en mode passif"),
  PASSIVE_OFF("%player% a quitt\u00e9 le mode passif"),
  CREATE_ORG("%player% a fond\u00e9 son organisation"),
  JOIN_ORG("%player% a rejoint une organisation"),
  LEAVE_ORG("%player% a quitt\u00e9 son organisation"),
  DIED_HUNGRY("%player% s'est \u00e9vanoui de faim"),
  HEALED("%player% a \u00e9t\u00e9 soign\u00e9 par les services m\u00e9dicaux"),
  RELEASED_JAIL("%player% a \u00e9t\u00e9 lib\u00e9r\u00e9 de prison"),
  ;

  private final String template;

  SinglePlayerTemplates(String template) {
    this.template = template;
  }

  /**
   * Format this template with a player.
   *
   * @param player The player performing the action
   * @return The formatted message
   */
  public String format(LivePlayer player) {
    return template.replace("%player%", player.getText());
  }

  /**
   * Format this template with a player name. Uses default player state of NORMAL.
   *
   * @param playerName The name of the player performing the action
   * @return The formatted message
   */
  public String format(String playerName) {
    return format(new LivePlayer(playerName, PlayerState.PASSIVE));
  }

  /**
   * Format this template with a player name and state.
   *
   * @param playerName The name of the player performing the action
   * @param state      The state of the player
   * @return The formatted message
   */
  public String format(String playerName, PlayerState state) {
    return format(new LivePlayer(playerName, state));
  }
}
