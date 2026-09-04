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
    if (params.length < 2) {
      gameClient.getHabbo().whisper(":utiliser <objet>", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("apply")
        .getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
    if (timeout != null) {
      gameClient.getHabbo().whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
      return true;
    }

    String itemName = String.join(" ", java.util.Arrays.copyOfRange(params, 1, params.length));
    RPItem item = data.getInventory().getSlotItem(itemName);
    if (item == null) {
      gameClient.getHabbo()
          .whisper("Vous ne poss\u00e9dez aucun objet nomm\u00e9 " + itemName + ".", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (applyItem(item, data, gameClient.getHabbo())) {
      data.updateLife();
      data.getInventory().removeItem(item, 1);
      data.getInventory().updateInventory(gameClient.getHabbo());
      gameClient.getHabbo().whisper("Vous avez utilis\u00e9 " + item.getDisplayName() + ".",
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
        habbo.whisper("Cet objet ne peut pas \u00eatre utilis\u00e9.", RoomChatMessageBubbles.ALERT);
        yield false;
      }
    };
  }

  private boolean applyHealthItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getHealth() >= data.getMaxHealth()) {
      habbo.whisper("Votre sant\u00e9 est d\u00e9j\u00e0 au maximum.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    habbo.whisper(getApplyMessage(item), RoomChatMessageBubbles.ALERT);
    useHealth(data, item.getExtraData());
    return true;
  }

  private static String getApplyMessage(RPItem item) {
    return Emulator.getTexts()
        .getValue(getApplyKey(item), "Commence \u00e0 utiliser " + item.getDisplayName() + "...");
  }

  private static @NotNull String getApplyKey(RPItem item) {
    return "features.apply." + item.getInteractionType();
  }

  private boolean applyEnergyItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getEnergy() >= data.getMaxEnergy()) {
      habbo.whisper("Votre \u00e9nergie est d\u00e9j\u00e0 au maximum.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    int targetEnergy = getItemAmount(10, item.getExtraData());
    int newEnergy = Math.min(data.getEnergy() + targetEnergy, data.getMaxEnergy());
    habbo.whisper(
        "Vous avez utilis\u00e9 " + item.getDisplayName() + " et r\u00e9cup\u00e9r\u00e9 "
            + (newEnergy - data.getEnergy()) + " point(s) d'\u00e9nergie.", RoomChatMessageBubbles.ALERT);
    data.setEnergy(newEnergy);
    return true;
  }

  private boolean applyShieldItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getShield() >= data.getMaxShield()) {
      habbo.whisper("Votre protection est d\u00e9j\u00e0 au maximum.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    int targetShield = getItemAmount(25, item.getExtraData());
    int newShield = Math.min(data.getShield() + targetShield, data.getMaxShield());
    habbo.whisper(getApplyMessage(item), RoomChatMessageBubbles.ALERT);
    data.setShield(newShield);
    return true;
  }

  private boolean applyFoodItem(RPItem item, RpAvatar data, Habbo habbo) {
    if (data.getHunger() >= data.getMaxHunger()) {
      habbo.whisper("Vous n'avez plus faim.", RoomChatMessageBubbles.ALERT);
      return false;
    }

    int restorationAmount = getItemAmount(10, item.getExtraData());
    int newHunger = Math.min(data.getHunger() + restorationAmount, data.getMaxHunger());

    habbo.whisper(
        "Vous avez consomm\u00e9 " + getFoodArticle(item) + " " + item.getDisplayName()
            + " et r\u00e9cup\u00e9r\u00e9 " + (newHunger - data.getHunger()) + " point(s) de faim.",
        RoomChatMessageBubbles.ALERT);

    data.setHunger(newHunger);
    return true;
  }

  private static String getFoodArticle(RPItem item) {
    return switch (item.getId()) {
      case 9, 10, 13, 18 -> "une"; // Pomme, Banane, Part de pizza, Pâtes
      default -> "un";
    };
  }

  private boolean applyDrugItem(RPItem item, RpAvatar data, Habbo habbo) {
    habbo.whisper(Emulator.getTexts().getValue(getApplyKey(item),
            "* Allume et consomme " + item.getDisplayName() + " *"),
        RoomChatMessageBubbles.ALERT);
    useDrug(item.getDisplayName().toLowerCase(), data, habbo, item.getExtraData().split("_"));
    return true;
  }

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
