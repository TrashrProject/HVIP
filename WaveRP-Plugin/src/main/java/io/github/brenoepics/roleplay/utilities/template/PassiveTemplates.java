package io.github.brenoepics.roleplay.utilities.template;


import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.LiveFeed.LivePlayer;
import io.github.brenoepics.roleplay.utilities.LiveFeed.PlayerState;

/**
 * Template keys requiring an actor and receiver (passive). Methods enforced: format(LivePlayer
 * actor, LivePlayer receiver) or format(String actorName, String receiverName)
 */
public enum PassiveTemplates {
  ACCEPT_OFFER("%actor% a accept\u00e9 une offre de %receiver%"),
  HEAL("%actor% a soign\u00e9 %receiver%"),
  PROMOTE("%actor% a promu %receiver% au grade %action%"),
  DEMOTE("%actor% a r\u00e9trograd\u00e9 %receiver% au grade %action%"),
  HIRE("%actor% a recrut\u00e9 %receiver% en tant que %action%"),
  FIRE("%actor% a licenci\u00e9 %receiver%"),
  ARREST("%actor% a envoy\u00e9 %receiver% en prison"),
  INVITE_ORG("%actor% a recrut\u00e9 %receiver% dans son organisation"),
  KICK_ORG("%actor% a exclu %receiver% de son organisation"),
  ESCORT_START("%actor% commence \u00e0 escorter %receiver%"),
  ESCORT_STOP("%actor% arr\u00eate d'escorter %receiver%");


  private final String template;

  PassiveTemplates(String template) {
    this.template = template;
  }

  /**
   * Format this template with an actor and receiver.
   *
   * @param actor    The player initiating the action
   * @param receiver The player receiving the action
   * @return The formatted message
   */
  public String format(LivePlayer actor, LivePlayer receiver) {
    return LiveFeed.formatPassive(template, actor, receiver, "");
  }

  /**
   * Format this template with an actor, receiver, and custom action.
   *
   * @param actor    The player initiating the action
   * @param receiver The player receiving the action
   * @param action   A custom action description
   * @return The formatted message
   */
  public String format(LivePlayer actor, LivePlayer receiver, String action) {
    return LiveFeed.formatPassive(template, actor, receiver, action);
  }

  /**
   * Format this template with actor and receiver names. Uses default player state of NORMAL for
   * both.
   *
   * @param actorName    The name of the player initiating the action
   * @param receiverName The name of the player receiving the action
   * @return The formatted message
   */
  public String format(String actorName, String receiverName) {
    return format(new LivePlayer(actorName, PlayerState.PASSIVE),
        new LivePlayer(receiverName, PlayerState.PASSIVE));
  }

  /**
   * Format this template with actor and receiver names, and action. Uses default player state of
   * NORMAL for both.
   *
   * @param actorName    The name of the player initiating the action
   * @param receiverName The name of the player receiving the action
   * @param action       A custom action description
   * @return The formatted message
   */
  public String format(String actorName, String receiverName, String action) {
    return format(new LivePlayer(actorName, PlayerState.PASSIVE),
        new LivePlayer(receiverName, PlayerState.PASSIVE), action);
  }

  /**
   * Format this template with actor and receiver info.
   *
   * @param actorName     The name of the player initiating the action
   * @param actorState    The state of the actor
   * @param receiverName  The name of the player receiving the action
   * @param receiverState The state of the receiver
   * @return The formatted message
   */
  public String format(String actorName, PlayerState actorState, String receiverName,
      PlayerState receiverState) {
    return format(new LivePlayer(actorName, actorState),
        new LivePlayer(receiverName, receiverState));
  }
}
