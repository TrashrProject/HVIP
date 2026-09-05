package io.github.brenoepics.roleplay.features.hospital;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Collection;
import java.util.Optional;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class HealingRunner implements Runnable {

  private static final long DELAY_MS = 5_000L;
  private static final long FULL_HEAL_DURATION_MS = 7 * 60 * 1000L;
  private static final long PROGRESS_MESSAGE_INTERVAL_MS = 60_000L;

  private static void processHospitalUsers(Collection<Habbo> entries) {
    for (Habbo habbo : entries) {
      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (isHealing(habbo, avatar)) {
        increaseHealth(habbo, avatar);
      }
    }
  }

  private static boolean isHealing(Habbo habbo, RpAvatar avatar) {
    if (habbo == null || avatar == null) {
      return false;
    }

    boolean knockedOut = RolePlay.getHospitalService().isHealing(habbo);

    if (isAtBed(habbo)) {
      boolean willWalk = !avatar.isDead();
      if (knockedOut) {
        RolePlay.getHospitalService().giveGlow(habbo);
        willWalk = false;
      }

      habbo.getRoomUnit().setCanWalk(willWalk);
    }

    return habbo.getHabboInfo().isOnline() && knockedOut;
  }

  private static boolean isAtBed(Habbo habbo) {
    return RolePlay.getHospitalService().isAtBed(habbo);
  }

  private static void increaseHealth(Habbo habbo, RpAvatar avatar) {
    long startedAt = RolePlay.getHospitalService().getHealingStartedAt(habbo);
    long elapsed = Math.max(0L, System.currentTimeMillis() - startedAt);
    double progress = Math.min(1.0d, (double) elapsed / FULL_HEAL_DURATION_MS);
    int newHealth = Math.min(avatar.getMaxHealth(),
        (int) Math.floor(avatar.getMaxHealth() * progress));
    // Never lower health if another treatment healed the player during regeneration.
    newHealth = Math.max(avatar.getHealth(), newHealth);
    avatar.setHealth(newHealth);

    int progressMinute = (int) (elapsed / PROGRESS_MESSAGE_INTERVAL_MS);
    if (progressMinute > 0
        && RolePlay.getHospitalService().markProgressMinute(habbo, progressMinute)) {
      habbo.whisper("Votre santé se régénère : " + newHealth + "/" + avatar.getMaxHealth() + ".");
    }

    if (newHealth < avatar.getMaxHealth()) {
      avatar.updateLife();
      return;
    }

    if (avatar.isDead()) {
      avatar.resetHungry();
    }

    avatar.heal();
    avatar.updateLife();
    avatar.updateDatabase();
    RolePlay.getHospitalService().finishHealing(habbo);
    habbo.whisper("Votre santé est complètement régénérée : " + avatar.getHealth() + "/"
        + avatar.getMaxHealth() + ".");
  }

  @Override
  public void run() {
    Optional<Room> hospital = RolePlay.getHospitalService().getHospital();
    if (hospital.isPresent()) {
      Collection<Habbo> entries = hospital.get().getHabbos();
      try {
        processHospitalUsers(entries);
      } catch (Exception e) {
        log.error("[ROLEPLAY] Error in HealingRunner: ", e);
      }
    }

    Emulator.getThreading().run(this, DELAY_MS);
    log.debug("[ROLEPLAY] Processed Hospital! Running again in {} ms", DELAY_MS);
  }
}
