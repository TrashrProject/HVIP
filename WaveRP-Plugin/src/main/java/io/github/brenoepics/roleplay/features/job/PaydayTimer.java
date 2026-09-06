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
 * Verse la paie aux joueurs réellement en service.
 *
 * Le montant vient directement de job_ranks.salary : chaque métier et chaque grade peut donc
 * avoir un salaire différent sans recoder le plugin. L'intervalle est configurable mais vaut
 * 10 minutes par défaut.
 */
public class PaydayTimer {

  private static final int DEFAULT_PAYDAY_MINUTES = 10;
  private final Timer timer = new Timer("ParadiseRP-Payday", true);

  public void init() {
    int paydayMinutes = Math.max(1,
        Emulator.getConfig().getInt("features.payday.timer_minutes", DEFAULT_PAYDAY_MINUTES));
    long periodMs = paydayMinutes * 60_000L;

    // On ne paie pas au démarrage du serveur : le premier passage arrive après un cycle complet.
    timer.scheduleAtFixedRate(new TimerTask() {
      @Override
      public void run() {
        payEmployees(paydayMinutes);
      }
    }, periodMs, periodMs);
  }

  private void payEmployees(int paydayMinutes) {
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
        // Sécurité pour les anciens joueurs / anciennes sessions.
        data.setLastPayday(now);
        continue;
      }

      int diffInMinutes = minutesSinceLastPayday(now, lastPayday);
      if (diffInMinutes < paydayMinutes) {
        continue;
      }

      JobRankEntity rank = data.getJobRankEntity();
      BigDecimal salary = rank.getSalary();
      if (salary == null || salary.signum() <= 0) {
        data.setLastPayday(now);
        continue;
      }

      int earned = salary.setScale(0, java.math.RoundingMode.HALF_UP).intValue();
      if (earned <= 0) {
        data.setLastPayday(now);
        continue;
      }

      data.setLastPayday(now);
      habbo.givePoints(200, earned);

      String jobName = data.getJobEntity() == null
          ? "Métier"
          : data.getJobEntity().getDisplayName();
      if (data.getJobEntity() != null
          && "hospital".equalsIgnoreCase(data.getJobEntity().getName())) {
        jobName = "EMS";
      }

      habbo.whisper(
          "Salaire reçu : +" + earned + " Bucks — " + jobName + " | " + rank.getDisplayName(),
          RoomChatMessageBubbles.ALERT);
    }

    toRemove.forEach(habbo -> RolePlay.getJobsManager().removeEmployee(habbo));
  }

  private static int minutesSinceLastPayday(Date now, Date lastPayday) {
    long diffInMillis = Math.max(0L, now.getTime() - lastPayday.getTime());
    return (int) (diffInMillis / 60_000L);
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
