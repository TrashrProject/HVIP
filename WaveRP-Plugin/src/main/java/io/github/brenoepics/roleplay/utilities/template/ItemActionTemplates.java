package io.github.brenoepics.roleplay.utilities.template;

import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring a player and item. Methods enforced: format(LivePlayer player, String
 * itemName) or format(String playerName, String itemName)
 */
public enum ItemActionTemplates {
  EQUIP("%player% whipped out their %item%"),
  UNEQUIP("%player% stashed their %item%"),
  SELL("%player% tried to hawk their %item%"),
  APPLY("%player% slapped on some %item%");


  private final String template;

  ItemActionTemplates(String template) {
    this.template = template;
  }

  /**
   * Format this template with a player and item.
   *
   * @param player   The player performing the action
   * @param itemName The name of the item
   * @return The formatted message
   */
  public String format(LivePlayer player, String itemName) {
    return template.replace("%player%", player.getText()).replace("%item%", itemName);
  }

  /**
   * Format this template with a player name and item. Uses default player state of NORMAL.
   *
   * @param playerName The name of the player performing the action
   * @param itemName   The name of the item
   * @return The formatted message
   */
  public String format(String playerName, String itemName) {
    return format(new LivePlayer(playerName, PlayerState.PASSIVE), itemName);
  }

  /**
   * Format this template with player info and item.
   *
   * @param playerName The name of the player performing the action
   * @param state      The state of the player
   * @param itemName   The name of the item
   * @return The formatted message
   */
  public String format(String playerName, PlayerState state, String itemName) {
    return format(new LivePlayer(playerName, state), itemName);
  }
}
