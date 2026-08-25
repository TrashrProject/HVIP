package io.github.brenoepics.roleplay.features.crime;

import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

public final class PoliceHandcuffService {
  private static final int TASER_EFFECT_ID = 53;
  private static final Set<Integer> HANDCUFFED = ConcurrentHashMap.newKeySet();

  private PoliceHandcuffService() {}

  public static boolean isHandcuffed(int userId) {
    return HANDCUFFED.contains(userId);
  }

  public static boolean handcuff(Habbo target) {
    int userId = target.getHabboInfo().getId();
    if (!HANDCUFFED.add(userId)) return false;
    enforce(target);
    return true;
  }

  public static boolean unhandcuff(Habbo target) {
    int userId = target.getHabboInfo().getId();
    if (!HANDCUFFED.remove(userId)) return false;

    PoliceTaserService.removeAfterUnhandcuff(target);
    boolean escorted = RolePlay.getEscortManager().isPrisonerEscorted(userId);
    target.getRoomUnit().setCanWalk(!escorted);
    Room room = target.getHabboInfo().getCurrentRoom();
    if (room != null && !escorted) room.giveEffect(target, 0, -1);
    return true;
  }

  public static void enforce(Habbo target) {
    if (target == null || !isHandcuffed(target.getHabboInfo().getId())) return;
    target.getRoomUnit().setCanWalk(false);
    Room room = target.getHabboInfo().getCurrentRoom();
    if (room != null) room.giveEffect(target, TASER_EFFECT_ID, -1);
  }

  public static void clear(int userId) {
    HANDCUFFED.remove(userId);
  }
}
