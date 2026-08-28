package io.github.brenoepics.roleplay.features.crime.prison;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Map.Entry;
import java.util.Optional;
import java.util.Set;
import lombok.extern.slf4j.Slf4j;
import org.jetbrains.annotations.NotNull;

@Slf4j
public class JailTimeRunner implements Runnable {

  private static final long DELAY_MS =
      (long) Emulator.getConfig().getInt("features.jail.delay.seconds", 15) * 1000;

  private static long lastMessage = Emulator.getIntUnixTimestamp();

  private static void processJailedUsers(Set<Entry<Integer, RpAvatar>> entries) {
    for (Entry<Integer, RpAvatar> entry : entries) {
      Habbo habbo = findHabbo(entry.getKey());
      if (isServingTime(habbo, entry.getValue())) {
        checkJailTime(habbo, entry.getValue());
      }
    }
  }

  private static Habbo findHabbo(Integer userId) {
    return Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
  }

  private static boolean isServingTime(Habbo habbo, RpAvatar avatar) {
    if (habbo == null || avatar == null || !avatar.isJailed()) {
      return false;
    }

    return habbo.getHabboInfo().isOnline() && isAtJail(habbo);
  }

  private static boolean isAtJail(Habbo habbo) {
    Optional<Room> jailRoom = RolePlay.getPrisonService().getJailRoom();
    return jailRoom.map(room -> room.equals(habbo.getHabboInfo().getCurrentRoom())).orElse(false);
  }

  private static void checkJailTime(Habbo habbo, RpAvatar avatar) {
    if (!avatar.isJailed()) {
      return;
    }

    long currentTime = Emulator.getIntUnixTimestamp();
    long jailEndTime = avatar.getJailTime();

    if (currentTime >= jailEndTime) {
      RolePlay.getPrisonService().releaseFromJail(habbo, avatar);
      return;
    }

    long remainingSeconds = jailEndTime - currentTime;
    long minutes = remainingSeconds / 60;
    long seconds = remainingSeconds % 60;

    if (currentTime - lastMessage >= 45) {
      lastMessage = currentTime;
      String msg = getMessage(minutes, seconds);
      habbo.whisper(msg);
    }
  }

  private static @NotNull String getMessage(long minutes, long seconds) {
    if (minutes > 0) {
      return String.format("Temps de prison restant : %d minute(s) et %d seconde(s).", minutes, seconds);
    } else {
      return String.format("Temps de prison restant : %d seconde(s).", seconds);
    }
  }


  @Override
  public void run() {
    Set<Entry<Integer, RpAvatar>> entries = RolePlay.getPrisonService().getJailedUsers().entrySet();
    try {
      processJailedUsers(entries);
    } catch (Exception e) {
      log.error("[ROLEPLAY] Error in JailTimeRunner: ", e);
    }

    Emulator.getThreading().run(this, DELAY_MS);
    log.debug("[ROLEPLAY] Processed Prison with {} users! Running again in {} ms", entries.size(),
        DELAY_MS);
  }
}
