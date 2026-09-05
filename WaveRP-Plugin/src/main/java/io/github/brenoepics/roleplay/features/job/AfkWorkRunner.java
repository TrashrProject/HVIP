package io.github.brenoepics.roleplay.features.job;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.ArrayList;
import java.util.Map.Entry;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class AfkWorkRunner implements Runnable {

  private static final long DELAY_MS = 1_000L;

  @Override
  public void run() {
    try {
      for (Entry<Habbo, RpAvatar> entry : new ArrayList<>(
          RolePlay.getAvatarManager().getCachedData().entrySet())) {
        stopWorkIfIdle(entry.getKey(), entry.getValue());
      }
    } catch (Exception exception) {
      log.error("[ROLEPLAY] Impossible de verifier les employes AFK", exception);
    }

    Emulator.getThreading().run(this, DELAY_MS);
  }

  private static void stopWorkIfIdle(Habbo habbo, RpAvatar avatar) {
    if (habbo == null || avatar == null || !avatar.isDuty()
        || !habbo.getHabboInfo().isOnline()) {
      return;
    }

    RoomUnit roomUnit = habbo.getRoomUnit();
    if (roomUnit == null || !roomUnit.isIdle()) {
      return;
    }

    RolePlay.getJobsManager().stopWork(habbo, avatar, JobsManager.StopReason.AFK);
    habbo.whisper("Vous avez arrêté de travailler car vous êtes AFK.",
        RoomChatMessageBubbles.ALERT);
    log.info("[ROLEPLAY] {} a ete retire du service pour inactivite",
        habbo.getHabboInfo().getUsername());
  }
}
