package io.github.brenoepics.roleplay.features.job;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Date;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

/**
 * Verse la paie aux joueurs réellement en service et affiche le temps restant chaque minute.
 * Le salaire vient de job_ranks.salary et la paie est versée en crédits Habbo.
 */
public class PaydayTimer {

  private static final int DEFAULT_PAYDAY_MINUTES = 10;
  private static final long CHECK_PERIOD_MS = 60_000L;
  private final Timer timer = new Timer("ParadiseRP-Payday", true);

  public void init() {
    int paydayMinutes = Math.max(1,
        Emulator.getConfig().getInt("features.payday.timer_minutes", DEFAULT_PAYDAY_MINUTES));

    // Vérification chaque minute pour afficher un vrai compte à rebours individuel.
    timer.scheduleAtFixedRate(new TimerTask() {
      @Override
      public void run() {
        processEmployees(paydayMinutes);
      }
    }, CHECK_PERIOD_MS, CHECK_PERIOD_MS);
  }

  private void processEmployees(int paydayMinutes) {
    List<Habbo> toRemove = new ArrayList<>();
    List<Habbo> onDutyEmployees = RolePlay.getJobsManager().getOnDutyEmployees().values().stream()
        .flatMap(Collection::stream)
        .distinct()
        .toList();

    Date now = new Date();
    for (Habbo habbo : onDutyEmployees) {
      RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (isUnavailable(habbo, data)) {
        toRemove.add(habbo);
        continue;
      }

      Date lastPayday = data.getLastPayday();
      if (lastPayday == null) {
        data.setLastPayday(now);
        sendCountdown(habbo, paydayMinutes);
        continue;
      }

      long elapsedMs = Math.max(0L, now.getTime() - lastPayday.getTime());
      long paydayMs = paydayMinutes * 60_000L;

      if (elapsedMs >= paydayMs) {
        payEmployee(habbo, data, now);
      } else {
        long remainingMs = paydayMs - elapsedMs;
        int remainingMinutes = (int) Math.ceil(remainingMs / 60_000.0);
        sendCountdown(habbo, Math.max(1, remainingMinutes));
      }
    }

    toRemove.forEach(habbo -> RolePlay.getJobsManager().removeEmployee(habbo));
  }

  private void payEmployee(Habbo habbo, RpAvatar data, Date now) {
    JobRankEntity rank = data.getJobRankEntity();
    BigDecimal salary = rank.getSalary();

    data.setLastPayday(now);

    if (salary == null || salary.signum() <= 0) {
      return;
    }

    int earned = salary.setScale(0, java.math.RoundingMode.HALF_UP).intValue();
    if (earned <= 0) {
      return;
    }

    habbo.giveCredits(earned);

    String jobName = data.getJobEntity() == null
        ? "Métier"
        : data.getJobEntity().getDisplayName();
    if (data.getJobEntity() != null
        && "hospital".equalsIgnoreCase(data.getJobEntity().getName())) {
      jobName = "EMS";
    }

    habbo.whisper(
        "Salaire reçu : +" + earned + " crédits — " + jobName + " | " + rank.getDisplayName(),
        RoomChatMessageBubbles.ALERT);
  }

  private static void sendCountdown(Habbo habbo, int minutes) {
    String unit = minutes > 1 ? "minutes" : "minute";
    habbo.whisper(
        "Prochaine paie dans " + minutes + " " + unit + ".",
        RoomChatMessageBubbles.ALERT);
  }

  private static boolean isUnavailable(Habbo habbo, RpAvatar data) {
    return habbo == null
        || data == null
        || !data.isDuty()
        || habbo.getRoomUnit() == null
        || habbo.getRoomUnit().isIdle()
        || data.getJobEntity() == null
        || data.getJobEntity().isUnemployed()
        || data.getJobRankEntity() == null
        || data.getJobRankEntity().equals(RolePlay.getJobService().getUnemployedRank());
  }
}
