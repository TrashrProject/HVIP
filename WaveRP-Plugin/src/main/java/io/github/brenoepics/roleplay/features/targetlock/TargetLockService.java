package io.github.brenoepics.roleplay.features.targetlock;

import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboStats;

public class TargetLockService {

  public static final String CLICKED_USER_LOCK = "clicked_user_lock";
  public static final String CLICKED_USER = "clicked_user";

  public static String getClickedUser(Habbo habbo) {
    return (String) habbo.getHabboStats().cache.getOrDefault(CLICKED_USER, "");
  }

  /**
   * Updates the lock status of the given Habbo.
   *
   * @param habbo The Habbo whose lock status is to be updated.
   * @return true if the lock was enabled, false if the lock was disabled.
   */
  public static boolean switchLock(Habbo habbo) {
    return isTargetLocked(habbo.getHabboStats()) ? disableLock(habbo) : enableLock(habbo);
  }

  public static boolean disableLock(Habbo habbo) {
    habbo.getHabboStats().cache.remove(CLICKED_USER_LOCK);
    habbo.whisper("You have unlocked your target");
    return false;
  }

  public static boolean enableLock(Habbo habbo) {
    habbo.getHabboStats().cache.put(CLICKED_USER_LOCK, "1");
    habbo.whisper("You have locked on to your target");
    return true;
  }


  public static boolean isTargetLocked(Habbo requester, Habbo target) {
    return target == requester || target.getHabboInfo().getCurrentRoom() == null
        || requester.getHabboInfo().getCurrentRoom() == null
        || target.getHabboInfo().getCurrentRoom() != requester.getHabboInfo().getCurrentRoom()
        || isTargetLocked(requester.getHabboStats());
  }

  public static boolean isTargetLocked(HabboStats stats) {
    return stats.cache.containsKey(CLICKED_USER_LOCK) && stats.cache.get(CLICKED_USER_LOCK)
        .equals("1");
  }

  public static void setClickedUser(Habbo requester, String target) {
    requester.getHabboStats().cache.put(CLICKED_USER, target);
  }

  public static boolean hasClickedUser(Habbo requester) {
	  return requester.getHabboStats().cache.containsKey(CLICKED_USER) && !requester.getHabboStats().cache.get(CLICKED_USER).equals("");
  }
}
