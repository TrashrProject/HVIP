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

  private static final long DELAY_MS =
      (long) Emulator.getConfig().getInt("features.heal.delay.seconds", 15) * 1000;

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

    return habbo.getHabboInfo().isOnline();
  }

  private static boolean isAtBed(Habbo habbo) {
    return RolePlay.getHospitalService().isAtBed(habbo);
  }

  private static void increaseHealth(Habbo habbo, RpAvatar avatar) {
    int healthGain = 5 + Emulator.getRandom().nextInt(5);
    int newHealth = Math.min(avatar.getMaxHealth(), avatar.getHealth() + healthGain);
    avatar.setHealth(newHealth);

    if (newHealth < avatar.getMaxHealth()) {
      avatar.updateLife();
      return;
    }

    if (avatar.isDead()) {
      avatar.resetHungry();
    }

    avatar.heal();
    avatar.updateDatabase();
    RolePlay.getHospitalService().finishHealing(habbo);
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
