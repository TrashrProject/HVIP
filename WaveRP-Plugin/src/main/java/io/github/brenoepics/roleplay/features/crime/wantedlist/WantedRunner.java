package io.github.brenoepics.roleplay.features.crime.wantedlist;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import java.sql.Timestamp;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class WantedRunner implements Runnable {

  private static long checkWantedStatuses() {
    long currentTime = System.currentTimeMillis();
    WantedSystemManager wantedList = getWantedSystemManager();
    long closestMillis = updateCrimes(wantedList, currentTime);

    return closestMillis == Long.MAX_VALUE ? 5000 : closestMillis;
  }

  private static long updateCrimes(WantedSystemManager wantedList, long currentMs) {
    long nextMillis = Long.MAX_VALUE;

    for (Map.Entry<Habbo, List<CriminalRecord>> crimes : wantedList.getOnlineUsersCrimes()
        .entrySet()) {

      Habbo habbo = crimes.getKey();
      Optional<Timestamp> endTimestamp = wantedList.getUserWantedEndTimestamp(crimes.getValue());

      if (endTimestamp.isPresent()) {
        long endTime = endTimestamp.get().getTime();
        if (endTime > currentMs && endTime < nextMillis) {
          nextMillis = endTime - currentMs;
        }

        checkExpiration(endTime, currentMs, habbo, wantedList);
      }
    }
    return nextMillis;
  }

  private static void checkExpiration(long endTime, long currentMs, Habbo habbo,
      WantedSystemManager wantedList) {
    boolean isFinished = endTime <= currentMs;
    if (!isFinished || habbo == null) {
      return;
    }

    boolean isEmpty = wantedList.updateUserCriminalRecord(habbo.getHabboInfo().getId());

    if (isEmpty) {
      habbo.whisper("Votre niveau de recherche a expir\u00e9. Vous n'\u00eates plus poursuivi.",
          RoomChatMessageBubbles.ALERT);
    }
  }

  private static WantedSystemManager getWantedSystemManager() {
    return RolePlay.getWantedManager();
  }

  @Override
  public void run() {
    long delay = checkWantedStatuses();
    if (delay < 0 || delay > 5000) {
      delay = 5000; // Minimum delay of 5 seconds in milliseconds
    }
    Emulator.getThreading().run(this, delay);
    log.debug("[ROLEPLAY] Checked Wanted List! Running again in {} ms", delay);
  }
}
