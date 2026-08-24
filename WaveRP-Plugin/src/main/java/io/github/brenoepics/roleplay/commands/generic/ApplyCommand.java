package io.github.brenoepics.roleplay.commands.generic;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.APPLY_TIMEOUT;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import org.jetbrains.annotations.NotNull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class ApplyCommand extends Command {

  private static final Logger LOGGER = LoggerFactory.getLogger(ApplyCommand.class);

  public ApplyCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
    if (params.length != 2) {
      gameClient.getHabbo().whisper(":apply <item>", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("apply")
        .getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
    if (timeout != null) {
      gameClient.getHabbo().whisper(
          "You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconds to use this command again!");
      return true;
    }

    RPItem item = data.getInventory().getSlotItem(params[1]);
    if (item == null) {
      gameClient.getHabbo()
          .whisper("You do not have any item called " + params[1], RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (applyItem(item, data, gameClient.getHabbo())) {
      data.updateLife();
      data.getInventory().removeItem(item, 1);
      data.getInventory().updateInventory(gameClient.getHabbo());
      gameClient.getHabbo().whisper("You successfully consumed a " + item.getDisplayName(),
          RoomChatMessageBubbles.ALERT);
      RolePlay.getCommandsCounter().getCoolDown("apply")
          .addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), APPLY_TIMEOUT);
    }

    return true;
  }

  private boolean applyItem(RPItem item, RpAvatar data, Habbo habbo) {
    return switch (item.getInteractionType()) {
      case "heal" -> applyHealthItem(item, data, habbo);
      case "energy" -> applyEnergyItem(item, data, habbo);
      case "shield" -> applyShieldItem(item, data, habbo);
      case "food" -> applyFoodItem(item, data, habbo);
      case "drug" -> applyDrugItem(item, data, habbo);
      default -> {
        habbo.whisper("This item cannot be applied", RoomChatMessageBubbles.ALERT);
        yield false;
      }
    };
  }

  /**
   * Applies a health item to restore player health
   */
  private boolean applyHealthItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getHealth() >= data.getMaxHealth()) {
      habbo.whisper("You already have full health", RoomChatMessageBubbles.ALERT);
      return false;
    }

    habbo.whisper(getApplyMessage(item), RoomChatMessageBubbles.ALERT);

    useHealth(data, item.getExtraData());
    return true;
  }

  private static String getApplyMessage(RPItem item) {
    return Emulator.getTexts()
        .getValue(getApplyKey(item), "Started consuming a " + item.getDisplayName() + "...");
  }

  private static @NotNull String getApplyKey(RPItem item) {
    return "features.apply." + item.getInteractionType();
  }

  /**
   * Applies an energy item to restore player energy
   */
  private boolean applyEnergyItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getEnergy() >= data.getMaxEnergy()) {
      habbo.whisper("You already have full energy", RoomChatMessageBubbles.ALERT);
      return false;
    }

    int targetEnergy = getItemAmount(10, item.getExtraData());
    int newEnergy = Math.min(data.getEnergy() + targetEnergy, data.getMaxEnergy());

    habbo.whisper(
        "You consumed " + item.getDisplayName() + " and gained " + (newEnergy - data.getEnergy())
            + " energy!", RoomChatMessageBubbles.ALERT);

    data.setEnergy(newEnergy);
    return true;
  }

  /**
   * Applies a shield item to restore the player shield
   */
  private boolean applyShieldItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getShield() >= data.getMaxShield()) {
      habbo.whisper("You already have full shield", RoomChatMessageBubbles.ALERT);
      return false;
    }

    int targetShield = getItemAmount(25, item.getExtraData());
    int newShield = Math.min(data.getShield() + targetShield, data.getMaxShield());

    habbo.whisper(getApplyMessage(item), RoomChatMessageBubbles.ALERT);

    data.setShield(newShield);
    return true;
  }

  /**
   * Applies a food item to restore player hunger
   */
  private boolean applyFoodItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getHunger() >= data.getMaxHunger()) {
      habbo.whisper("You already have full hunger", RoomChatMessageBubbles.ALERT);
      return false;
    }

    int restorationAmount = getItemAmount(10, item.getExtraData());
    int newHunger = Math.min(data.getHunger() + restorationAmount, data.getMaxHunger());

    habbo.whisper(
        "You consumed " + item.getDisplayName() + " and restored " + (newHunger - data.getHunger())
            + " hunger!", RoomChatMessageBubbles.ALERT);

    data.setHunger(newHunger);
    return true;
  }

  /**
   * Applies a drug item with special effects
   */
  private boolean applyDrugItem(RPItem item, RpAvatar data, Habbo habbo) {
    habbo.whisper(Emulator.getTexts().getValue(getApplyKey(item),
            "Lights up a joint and smokes " + item.getDisplayName() + " - you're high!"),
        RoomChatMessageBubbles.ALERT);

    useDrug(item.getInteractionType(), data, habbo, item.getExtraData().split("_"));
    return true;
  }

  /**
   * Helper method to get item effect amount with a default fallback
   */
  private int getItemAmount(int defaultAmount, String extraData) {
    try {
      return Integer.parseInt(extraData);
    } catch (NumberFormatException e) {
      return defaultAmount;
    }
  }

  private void useDrug(String type, RpAvatar data, Habbo habbo, String[] params) {
    if (params.length != 2) {
      LOGGER.error("Drug extra data is broken should be: <type>_<duration>");
      return;
    }
    int duration = getItemAmount(100, params[1]);

    switch (params[0]) {
      case "fastwalk":
        habbo.getRoomUnit().setFastWalk(true);
        Emulator.getThreading().run(() -> habbo.getRoomUnit().setFastWalk(false), duration * 1000L);
        break;
      case "strength":
        data.setStrength(
            Emulator.getConfig().getDouble("nahabbo.features.drugs." + type + ".strength", 1.5));
        Emulator.getThreading().run(() -> data.setStrength(1), duration * 1000L);
        break;
      default:
        LOGGER.error("Drug type not found: {}", params[0]);
        break;
    }
  }

  private void useHealth(RpAvatar data, String extradata) {
    int currentHealth = data.getHealth();
    int maxHealth = data.getMaxHealth();

    int amountToHeal = getItemAmount(100, extradata);

    if (currentHealth >= maxHealth || amountToHeal <= 0) {
      return;
    }

    int targetHealth = Math.min(currentHealth + amountToHeal, maxHealth);
    int totalToRestore = targetHealth - currentHealth;

    int chunk = 5;
    int fullSteps = totalToRestore / chunk;
    int remainder = totalToRestore % chunk;

    final int startHealth = currentHealth;

    for (int i = 0; i < fullSteps; i++) {
      final int stepIndex = i + 1;
      Emulator.getThreading().run(() -> {
        int stepHealth = Math.min(startHealth + (stepIndex * chunk), maxHealth);
        data.setHealth(stepHealth);
        data.updateClientData();
      }, 1000L + (i * 1000L));
    }

    if (remainder > 0) {
      long finalDelay = 1000L + (fullSteps * 1000L);
      Emulator.getThreading().run(() -> {
        int stepHealth = Math.min(startHealth + (fullSteps * chunk) + remainder, maxHealth);
        data.setHealth(stepHealth);
        data.updateClientData();
      }, finalDelay);
    }
  }


}
