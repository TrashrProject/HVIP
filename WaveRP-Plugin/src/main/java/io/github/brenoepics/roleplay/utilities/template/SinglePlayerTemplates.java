package io.github.brenoepics.roleplay.utilities.template;

import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring a single player. Methods enforced: format(String playerName) or
 * format(LivePlayer player)
 */
public enum SinglePlayerTemplates {
  START_WORK("%player% clocked in for the grind"),
  STOP_WORK("%player% made a break for freedom"),
  HELP("%player% screamed for backup"),
  PASSIVE_ON("%player% went all zen mode"),
  PASSIVE_OFF("%player% got ready to rumble again"),
  CREATE_ORG("%player% established their criminal empire"),
  JOIN_ORG("%player% pledged allegiance to the crew"),
  LEAVE_ORG("%player% went rogue from their crew"),
  DIED_HUNGRY("%player% was knocked out from hunger"),
  HEALED("%player% has been healed at the hospital"),
  RELEASED_JAIL("%player% has been released from jail"),
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
