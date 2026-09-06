package io.github.brenoepics.roleplay.features.job;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.ConcurrentHashMap;

/** Systeme de paie ParadiseRP. Les messages de paie sont prives au joueur concerne. */
public class PaydayTimer {

  private static final int DEFAULT_PAYDAY_MINUTES = 10;
  private static final long TICK_MS = 60_000L;

  private final Timer timer = new Timer("ParadiseRP-Payday", true);
  private final Map<Integer, Long> nextPayAt = new ConcurrentHashMap<>();

  public void init() {
    timer.scheduleAtFixedRate(new TimerTask() {
      @Override
      public void run() {
        tick();
      }
    }, TICK_MS, TICK_MS);
  }

  public void onWorkStarted(Habbo habbo, RpAvatar data) {
    if (habbo == null || data == null || !data.isDuty()) {
      return;
    }

    int minutes = getPaydayMinutes();
    long next = System.currentTimeMillis() + minutes * 60_000L;
    nextPayAt.put(habbo.getHabboInfo().getId(), next);
    sendPrivateMessage(habbo, "Prochaine paie dans " + minutes + " minutes.");
  }

  public void onWorkStopped(Habbo habbo) {
    if (habbo != null && habbo.getHabboInfo() != null) {
      nextPayAt.remove(habbo.getHabboInfo().getId());
    }
  }

  private void tick() {
    List<Habbo> employees = new ArrayList<>();
    RolePlay.getJobsManager().getOnDutyEmployees().values().stream()
        .flatMap(Collection::stream)
        .distinct()
        .forEach(employees::add);

    long now = System.currentTimeMillis();
    int paydayMinutes = getPaydayMinutes();
    long cycleMs = paydayMinutes * 60_000L;

    for (Habbo habbo : employees) {
      RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (!isEligible(habbo, data)) {
        onWorkStopped(habbo);
        continue;
      }

      int userId = habbo.getHabboInfo().getId();
      long dueAt = nextPayAt.computeIfAbsent(userId, ignored -> now + cycleMs);

      if (now >= dueAt) {
        pay(habbo, data);
        nextPayAt.put(userId, now + cycleMs);
        continue;
      }

      long remainingMs = dueAt - now;
      int remainingMinutes = (int) Math.ceil(remainingMs / 60_000.0);
      sendPrivateMessage(habbo,
          "Prochaine paie dans " + remainingMinutes + " "
              + (remainingMinutes > 1 ? "minutes" : "minute") + ".");
    }
  }

  private void pay(Habbo habbo, RpAvatar data) {
    JobRankEntity rank = data.getJobRankEntity();
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
        && habbo.getRoomUnit() != null
        && !habbo.getRoomUnit().isIdle();
  }

  private static void sendPrivateMessage(Habbo habbo, String message) {
    if (habbo == null) {
      return;
    }
    habbo.whisper(message, RoomChatMessageBubbles.NORMAL);
  }
}
