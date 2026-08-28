package io.github.brenoepics.roleplay.utilities;

import static org.junit.jupiter.api.Assertions.assertEquals;

import io.github.brenoepics.roleplay.utilities.template.CombatTemplates;
import org.junit.jupiter.api.Test;

class LiveFeedTest {

  @Test
  void testFormatCombat() {
    // Given
    LiveFeed.LivePlayer attacker = new LiveFeed.LivePlayer("Player1",
        LiveFeed.PlayerState.ATTACKER);
    LiveFeed.LivePlayer victim = new LiveFeed.LivePlayer("Player2", LiveFeed.PlayerState.VICTIM);
    String template = "%attacker% defeated %victim%!";

    // When
    String result = LiveFeed.formatCombat(template, attacker, victim);

    // Then
    String expected = "<span style=\"color: #00ff00;\">Player1</span> defeated <span style=\"color: #ff0000;\">Player2</span>!";
    assertEquals(expected, result);
  }

  @Test
  void testFormatAction() {
    // Given
    LiveFeed.LivePlayer player = new LiveFeed.LivePlayer("Helper", LiveFeed.PlayerState.PASSIVE);
    String template = "%player% performed an action: %action%";
    String action = "helping others";

    // When
    String result = LiveFeed.formatAction(template, player, action);

    // Then
    String expected = "<span style=\"color: #0099ff;\">Helper</span> performed an action: helping others";
    assertEquals(expected, result);
  }

  @Test
  void testFormatPassive() {
    // Given
    LiveFeed.LivePlayer actor = new LiveFeed.LivePlayer("Giver", LiveFeed.PlayerState.PASSIVE);
    LiveFeed.LivePlayer receiver = new LiveFeed.LivePlayer("Receiver", LiveFeed.PlayerState.VICTIM);
    String template = "%actor% gave an item to %receiver% for %action%";
    String action = "charitable donations";

    // When
    String result = LiveFeed.formatPassive(template, actor, receiver, action);

    // Then
    String expected = "<span style=\"color: #0099ff;\">Giver</span> gave an item to <span style=\"color: #ff0000;\">Receiver</span> for charitable donations";
    assertEquals(expected, result);
  }

  @Test
  void testAlertWithCombatKillTemplate() {
    // Given
    String killer = "fakeuser2";
    String victim = "fakeuser";
    String msg = CombatTemplates.KILL.format(killer, victim);

    String expected = "<span style=\"color: #00ff00;\">fakeuser2</span> brutally took out <span style=\"color: #ff0000;\">fakeuser</span>";


  }
}