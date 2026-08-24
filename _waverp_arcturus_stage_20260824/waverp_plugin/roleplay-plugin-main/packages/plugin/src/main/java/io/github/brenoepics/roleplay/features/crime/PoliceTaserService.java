package io.github.brenoepics.roleplay.features.crime;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

public final class PoliceTaserService {
  private static final int EFFECT_ID = 53;
  private static final Map<Integer, UUID> ACTIVE = new ConcurrentHashMap<>();

  private PoliceTaserService() {}

  public static boolean isTased(int userId) { return ACTIVE.containsKey(userId); }

  public static boolean taser(Habbo target) {
    int userId = target.getHabboInfo().getId();
    if (ACTIVE.containsKey(userId)) return false;
    UUID token = UUID.randomUUID();
    ACTIVE.put(userId, token);
    Room room = target.getHabboInfo().getCurrentRoom();
    if (room != null) room.giveEffect(target, EFFECT_ID, -1);
    target.getRoomUnit().setCanWalk(false);
    int seconds = Math.max(40, Emulator.getConfig().getInt("features.police.tazor.duration_seconds", 40));
    Emulator.getThreading().run(() -> recover(target, token, true), seconds * 1000L);
    return true;
  }

  public static boolean remove(Habbo target) {
    if (ACTIVE.remove(target.getHabboInfo().getId()) == null) return false;
    finish(target, false);
    return true;
  }

  private static void recover(Habbo target, UUID token, boolean announce) {
    if (!ACTIVE.remove(target.getHabboInfo().getId(), token)) return;
    finish(target, announce);
  }

  private static void finish(Habbo target, boolean announce) {
    boolean escorted = RolePlay.getEscortManager().isPrisonerEscorted(target.getHabboInfo().getId());
    target.getRoomUnit().setCanWalk(!escorted);
    Room room = target.getHabboInfo().getCurrentRoom();
    if (room != null && !escorted) room.giveEffect(target, 0, -1);
    if (announce && room != null) {
      target.shout("* " + target.getHabboInfo().getUsername() + " recupere de l'effet du taser *", RoomChatMessageBubbles.YELLOW);
    }
  }
}
