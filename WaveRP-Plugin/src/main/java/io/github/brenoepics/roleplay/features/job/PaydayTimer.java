package io.github.brenoepics.roleplay.features.job;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Date;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

public class PaydayTimer {

  private final Timer timer = new Timer();

  public PaydayTimer() {
  }

  public void init() {
    timer.scheduleAtFixedRate(new TimerTask() {
      @Override
      public void run() {
        payEmployees();
      }
    }, 0, (long) Emulator.getConfig().getInt("features.payday.timer_minutes", 10) * 60
        * 1000); // 10 minutes in milliseconds
  }

  private void payEmployees() {
    List<Habbo> toRemove = new ArrayList<>();
    List<Habbo> onDutyEmployees = RolePlay.getJobsManager().getOnDutyEmployees().values().stream()
        .flatMap(Collection::stream).distinct().toList();
    Date now = new Date();
    for (Habbo h : onDutyEmployees) {
      RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(h);
      if (isUnavailable(h, data)) {
        toRemove.add(h);
        continue;
      }

      int diffInMinutes = minutesSinceLastPayday(now, data);

      if (diffInMinutes < 10) {
        h.whisper("You have to wait " + (10 - diffInMinutes) + " minutes to receive your paycheck",
            RoomChatMessageBubbles.ALERT);
      } else {
        double earned =
            ((double) diffInMinutes / 10) * data.getJobRankEntity().getSalary().doubleValue();
        data.setLastPayday(now);
        h.givePoints(200, (int) Math.round(earned));
        h.whisper("You received your paycheck. +" + (int) Math.round(earned) + " Bucks",
            RoomChatMessageBubbles.ALERT);
      }

    }

    toRemove.forEach(h -> RolePlay.getJobsManager().removeEmployee(h));
  }

  private static int minutesSinceLastPayday(Date now, RpAvatar data) {
    long diffInMillis = now.getTime() - data.getLastPayday().getTime();
    return (int) (diffInMillis / (60 * 1000));
  }

  private static boolean isUnavailable(Habbo h, RpAvatar data) {
    return data == null || !data.isDuty() || h.getRoomUnit().isIdle()
        || data.getJobRankEntity().equals(RolePlay.getJobService().getUnemployedRank());
  }
}
