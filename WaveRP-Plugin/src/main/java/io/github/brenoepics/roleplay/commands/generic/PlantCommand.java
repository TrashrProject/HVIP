package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.util.concurrent.ThreadLocalRandom;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class PlantCommand extends Command {

  private static final int SEED_ID = 6118;
  private static final int CARROT_ID = 6121;
  private static final int GROW_SECONDS = 45;

  public PlantCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (gameClient == null || gameClient.getHabbo() == null) {
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
    if (avatar == null) {
      return true;
    }

    if (avatar.isPassive()) {
      gameClient.getHabbo().whisper(
          "Vous ne pouvez pas cultiver en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String key = params.length > 0 ? params[0] : "planter";
    if ("recolter".equalsIgnoreCase(key) || "récolter".equalsIgnoreCase(key)
        || "harvest".equalsIgnoreCase(key)) {
      return harvest(gameClient, avatar);
    }

    return plant(gameClient, avatar);
  }

  private boolean plant(GameClient gameClient, RpAvatar avatar) {
    RPItem seed = RolePlay.getItemManager().getItemById(SEED_ID);
    if (seed == null) {
      gameClient.getHabbo().whisper(
          "Le système de graines n'est pas encore initialisé en base de données.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (avatar.getInventory().getSlotItem(SEED_ID) == null) {
      gameClient.getHabbo().whisper(
          "Vous devez avoir une Graine dans votre inventaire.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int userId = gameClient.getHabbo().getHabboInfo().getId();
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      connection.setAutoCommit(false);
      try {
        try (PreparedStatement check = connection.prepareStatement(
            "SELECT GREATEST(0, TIMESTAMPDIFF(SECOND, NOW(), ready_at)) AS remaining "
                + "FROM paradise_crops WHERE user_id = ? FOR UPDATE")) {
          check.setInt(1, userId);
          try (ResultSet result = check.executeQuery()) {
            if (result.next()) {
              int remaining = result.getInt("remaining");
              connection.rollback();
              gameClient.getHabbo().whisper(
                  remaining > 0
                      ? "Une culture est déjà en cours. Récolte possible dans " + remaining
                          + " seconde(s)."
                      : "Votre culture est prête. Utilisez :recolter.",
                  RoomChatMessageBubbles.ALERT);
              return true;
            }
          }
        }

        try (PreparedStatement insert = connection.prepareStatement(
            "INSERT INTO paradise_crops (user_id, crop_key, planted_at, ready_at) "
                + "VALUES (?, 'carrot', NOW(), DATE_ADD(NOW(), INTERVAL ? SECOND))")) {
          insert.setInt(1, userId);
          insert.setInt(2, GROW_SECONDS);
          insert.executeUpdate();
        }
        connection.commit();
      } catch (Exception exception) {
        connection.rollback();
        throw exception;
      } finally {
        connection.setAutoCommit(true);
      }
    } catch (Exception exception) {
      log.error("Unable to plant seed for user {}", userId, exception);
      gameClient.getHabbo().whisper(
          "Impossible de planter cette graine pour le moment.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    avatar.getInventory().removeItem(seed, 1);
    avatar.getInventory().updateInventory(gameClient.getHabbo());
    gameClient.getHabbo().whisper(
        "Graine plantée. Revenez dans " + GROW_SECONDS + " secondes puis utilisez :recolter.",
        RoomChatMessageBubbles.ALERT);
    return true;
  }

  private boolean harvest(GameClient gameClient, RpAvatar avatar) {
    RPItem carrot = RolePlay.getItemManager().getItemById(CARROT_ID);
    if (carrot == null) {
      gameClient.getHabbo().whisper(
          "Le système de récolte n'est pas encore initialisé en base de données.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    int userId = gameClient.getHabbo().getHabboInfo().getId();
    int quantity = ThreadLocalRandom.current().nextInt(2, 5);

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      connection.setAutoCommit(false);
      try {
        int remaining;
        try (PreparedStatement select = connection.prepareStatement(
            "SELECT TIMESTAMPDIFF(SECOND, NOW(), ready_at) AS remaining "
                + "FROM paradise_crops WHERE user_id = ? FOR UPDATE")) {
          select.setInt(1, userId);
          try (ResultSet result = select.executeQuery()) {
            if (!result.next()) {
              connection.rollback();
              gameClient.getHabbo().whisper(
                  "Vous n'avez aucune culture en cours. Utilisez :planter.",
                  RoomChatMessageBubbles.ALERT);
              return true;
            }
            remaining = result.getInt("remaining");
          }
        }

        if (remaining > 0) {
          connection.rollback();
          gameClient.getHabbo().whisper(
              "La culture n'est pas encore prête. Attendez " + remaining + " seconde(s).",
              RoomChatMessageBubbles.ALERT);
          return true;
        }

        try (PreparedStatement delete = connection.prepareStatement(
            "DELETE FROM paradise_crops WHERE user_id = ?")) {
          delete.setInt(1, userId);
          delete.executeUpdate();
        }
        connection.commit();
      } catch (Exception exception) {
        connection.rollback();
        throw exception;
      } finally {
        connection.setAutoCommit(true);
      }
    } catch (Exception exception) {
      log.error("Unable to harvest crop for user {}", userId, exception);
      gameClient.getHabbo().whisper(
          "Impossible de récolter pour le moment.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    avatar.getInventory().addItem(gameClient.getHabbo(), carrot, quantity);
    gameClient.getHabbo().whisper(
        "Récolte terminée : " + quantity + " × Carotte.", RoomChatMessageBubbles.ALERT);
    return true;
  }
}
