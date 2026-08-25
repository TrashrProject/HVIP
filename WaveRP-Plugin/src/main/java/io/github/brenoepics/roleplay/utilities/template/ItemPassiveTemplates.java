package io.github.brenoepics.roleplay.utilities.template;

import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring an actor, receiver, and item. Methods enforced: format(LivePlayer
 * actor, LivePlayer receiver, String itemName) or format(String actorName, String receiverName,
 * String itemName)
 */
public enum ItemPassiveTemplates {
  OFFER("%actor% propose %item% \u00e0 %receiver%");

  private final String template;

  ItemPassiveTemplates(String template) {
    this.template = template;
  }

  /**
   * Format this template with an actor, receiver, and item.
   *
   * @param actor    The player initiating the action
   * @param receiver The player receiving the action
   * @param itemName The name of the item
   * @return The formatted message
   */
  public String format(LivePlayer actor, LivePlayer receiver, String itemName) {
    return template.replace("%actor%", actor.getText()).replace("%receiver%", receiver.getText())
        .replace("%item%", itemName);
  }

  /**
   * Format this template with actor and receiver names, and item. Uses default player state of
   * NORMAL for both.
   *
   * @param actorName    The name of the player initiating the action
   * @param receiverName The name of the player receiving the action
   * @param itemName     The name of the item
   * @return The formatted message
   */
  public String format(String actorName, String receiverName, String itemName) {
    return format(new LivePlayer(actorName, PlayerState.PASSIVE),
        new LivePlayer(receiverName, PlayerState.PASSIVE), itemName);
  }

  /**
   * Format this template with actor and receiver info, and item.
   *
   * @param actorName     The name of the player initiating the action
   * @param actorState    The state of the actor
   * @param receiverName  The name of the player receiving the action
   * @param receiverState The state of the receiver
   * @param itemName      The name of the item
   * @return The formatted message
   */
  public String format(String actorName, PlayerState actorState, String receiverName,
      PlayerState receiverState, String itemName) {
    return format(new LivePlayer(actorName, actorState),
        new LivePlayer(receiverName, receiverState), itemName);
  }
}
