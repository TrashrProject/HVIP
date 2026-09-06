package io.github.brenoepics.roleplay.features.job;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Systeme de paie ParadiseRP.
 * Chaque prise de service demarre un compte a rebours individuel et prive.
 */
public class PaydayTimer {

  private static final int DEFAULT_PAYDAY_MINUTES = 10;
  private static final long ONE_MINUTE_MS = 60_000L;

  /** Identifiant du cycle actif de chaque joueur. */
  private final Map<Integer, String> activeCycles = new ConcurrentHashMap<>();

  public void init() {
    // Plus de timer global : chaque joueur possede son propre cycle de paie.
  }

  public void onWorkStarted(Habbo habbo, RpAvatar data) {
    if (!isEligible(habbo, data)) {
      return;
    }

    int userId = habbo.getHabboInfo().getId();
    int paydayMinutes = getPaydayMinutes();
    String cycleId = UUID.randomUUID().toString();

    activeCycles.put(userId, cycleId);
    sendPrivateMessage(habbo,
        "Prochaine paie dans " + paydayMinutes + " "
            + (paydayMinutes > 1 ? "minutes" : "minute") + ".");

    scheduleNextTick(habbo, userId, cycleId, paydayMinutes - 1);
  }

  public void onWorkStopped(Habbo habbo) {
    if (habbo != null && habbo.getHabboInfo() != null) {
      activeCycles.remove(habbo.getHabboInfo().getId());
    }
  }

  private void scheduleNextTick(Habbo habbo, int userId, String cycleId,
      int remainingMinutes) {
    Emulator.getThreading().run(
        () -> processTick(habbo, userId, cycleId, remainingMinutes),
        ONE_MINUTE_MS);
  }

  private void processTick(Habbo habbo, int userId, String cycleId,
      int remainingMinutes) {
    if (!cycleId.equals(activeCycles.get(userId))) {
      return;
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (!isEligible(habbo, data)) {
      activeCycles.remove(userId, cycleId);
      return;
    }

    if (remainingMinutes > 0) {
      sendPrivateMessage(habbo,
          "Prochaine paie dans " + remainingMinutes + " "
              + (remainingMinutes > 1 ? "minutes" : "minute") + ".");
      scheduleNextTick(habbo, userId, cycleId, remainingMinutes - 1);
      return;
    }

    pay(habbo, data);

    if (!cycleId.equals(activeCycles.get(userId))) {
      return;
    }

    int paydayMinutes = getPaydayMinutes();
    sendPrivateMessage(habbo,
        "Prochaine paie dans " + paydayMinutes + " "
            + (paydayMinutes > 1 ? "minutes" : "minute") + ".");
    scheduleNextTick(habbo, userId, cycleId, paydayMinutes - 1);
  }

  private void pay(Habbo habbo, RpAvatar data) {
    JobRankEntity rank = data.getJobRankEntity();
    if (rank == null) {
      return;
    }

    BigDecimal salary = rank.getSalary();
    if (salary == null || salary.signum() <= 0) {
      return;
    }

    int earned = salary.setScale(0, RoundingMode.HALF_UP).intValue();
    if (earned <= 0) {
      return;
    }

    habbo.giveCredits(earned);

    String jobName = data.getJobEntity().getDisplayName();
    if ("hospital".equalsIgnoreCase(data.getJobEntity().getName())) {
      jobName = "EMS";
    }

    sendPrivateMessage(habbo,
        "Salaire reçu : +" + earned + " crédits — " + jobName + " | "
            + rank.getDisplayName());
  }

  private static int getPaydayMinutes() {
    return Math.max(1,
        Emulator.getConfig().getInt("features.payday.timer_minutes", DEFAULT_PAYDAY_MINUTES));
  }

  private static boolean isEligible(Habbo habbo, RpAvatar data) {
    return habbo != null
        && habbo.getHabboInfo() != null
        && data != null
        && data.isDuty()
        && data.getJobEntity() != null
        && !data.getJobEntity().isUnemployed()
        && data.getJobRankEntity() != null
        && !data.getJobRankEntity().equals(RolePlay.getJobService().getUnemployedRank())
        && habbo.getRoomUnit() != null;
  }

  private static void sendPrivateMessage(Habbo habbo, String message) {
    if (habbo != null) {
      habbo.whisper(message, RoomChatMessageBubbles.NORMAL);
    }
  }
}
