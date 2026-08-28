package io.github.brenoepics.roleplay.features.user;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.template.SinglePlayerTemplates;
import java.util.Map.Entry;
import lombok.extern.slf4j.Slf4j;
import org.jetbrains.annotations.NotNull;

@Slf4j
public class HungerRunner implements Runnable {

  private static final long DELAY_MS =
      (long) Emulator.getConfig().getInt("features.hunger.delay.minutes", 1) * 60 * 1000;
  private static final String HUNGER_MESSAGE = "Votre faim a diminué de %amount%. Mangez pour la récupérer ! (%current%/%max%)";
  public static final String MISSING_ENERGY = "Vous n'avez pas assez d'énergie pour cette action.";

  private static void processOnlinePlayers() {
    for (Entry<Habbo, RpAvatar> entry : RolePlay.getAvatarManager().getCachedData().entrySet()) {
      if (isOnline(entry) && !entry.getKey().hasPermission("acc_infinite_hunger")) {
        decreaseHunger(entry.getKey(), entry.getValue());
      }
    }
  }

  private static boolean isOnline(Entry<Habbo, RpAvatar> entry) {
    if (entry == null) {
      return false;
    }

    Habbo habbo = entry.getKey();
    RpAvatar avatar = entry.getValue();
    return habbo != null && avatar != null && !avatar.isDead() && !avatar.isJailed()
        && habbo.getHabboInfo().isOnline();
  }

  private static void decreaseHunger(Habbo habbo, RpAvatar avatar) {
    if (avatar.isDead()) {
      avatar.updateDatabase();
      return;
    }

    int hungerLoss = 1 + Emulator.getRandom().nextInt(3);
    int newHunger = Math.max(0, avatar.getHunger() - hungerLoss);

    avatar.setHunger(newHunger);
    String msg = getMessage(avatar, hungerLoss, newHunger);
    habbo.whisper(msg);

    if (newHunger == 0) {
      avatar.setHealth(0);

      LiveFeed.sendGlobalAlert(LiveFeed.alert(
          SinglePlayerTemplates.DIED_HUNGRY.format(habbo.getHabboInfo().getUsername())));

      avatar.makeDead();
    }

    avatar.updateDatabase();
  }

  private static @NotNull String getMessage(RpAvatar avatar, int hungerLoss, int newHunger) {
    return HUNGER_MESSAGE.replace("%amount%", String.valueOf(hungerLoss))
        .replace("%current%", String.valueOf(newHunger))
        .replace("%max%", String.valueOf(avatar.getMaxHunger()));
  }

  @Override
  public void run() {
    try {
      processOnlinePlayers();
    } catch (Exception e) {
      log.error("[ROLEPLAY] Error in HungerRunner: ", e);
    }

    Emulator.getThreading().run(this, DELAY_MS);
    log.debug("[ROLEPLAY] Processed player hunger! Running again in {} ms", DELAY_MS);
  }
}
