package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ThreadLocalRandom;

public class FishingCommand extends Command {

  private static final int FISHING_ROD_ID = 6115;
  private static final int TUNA_ID = 6119;
  private static final int SALMON_ID = 6120;
  private static final long COOLDOWN_MS = 15_000L;
  private static final ConcurrentHashMap<Integer, Long> COOLDOWNS = new ConcurrentHashMap<>();

  public FishingCommand(String permission, String[] keys) {
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
          "Vous ne pouvez pas pêcher en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (gameClient.getHabbo().getHabboInfo().getCurrentRoom() == null) {
      gameClient.getHabbo().whisper(
          "Vous devez être dans un appartement pour pêcher.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RPItem rod = RolePlay.getItemManager().getItemById(FISHING_ROD_ID);
    if (rod == null || avatar.getInventory().getSlotItem(FISHING_ROD_ID) == null) {
      gameClient.getHabbo().whisper(
          "Il vous faut une Canne à pêche dans votre inventaire.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int userId = gameClient.getHabbo().getHabboInfo().getId();
    long now = System.currentTimeMillis();
    long nextAllowed = COOLDOWNS.getOrDefault(userId, 0L);
    if (nextAllowed > now) {
      long seconds = Math.max(1L, (nextAllowed - now + 999L) / 1000L);
      gameClient.getHabbo().whisper(
          "Patientez encore " + seconds + " seconde(s) avant de relancer la ligne.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    RPItem tuna = RolePlay.getItemManager().getItemById(TUNA_ID);
    RPItem salmon = RolePlay.getItemManager().getItemById(SALMON_ID);
    if (tuna == null || salmon == null) {
      gameClient.getHabbo().whisper(
          "Le système de pêche n'est pas encore initialisé en base de données.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    COOLDOWNS.put(userId, now + COOLDOWN_MS);

    boolean caughtSalmon = ThreadLocalRandom.current().nextInt(100) < 30;
    RPItem reward = caughtSalmon ? salmon : tuna;
    int quantity = ThreadLocalRandom.current().nextInt(100) < 15 ? 2 : 1;

    avatar.getInventory().addItem(gameClient.getHabbo(), reward, quantity);
    gameClient.getHabbo().whisper(
        "Vous avez pêché " + quantity + " × " + reward.getDisplayName() + ".",
        RoomChatMessageBubbles.ALERT);
    return true;
  }
}
