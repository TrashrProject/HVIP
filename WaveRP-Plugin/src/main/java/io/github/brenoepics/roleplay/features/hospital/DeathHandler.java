package io.github.brenoepics.roleplay.features.hospital;

import static io.github.brenoepics.roleplay.features.hospital.HospitalService.HOSPITAL_ROOM_ID;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

public class DeathHandler {

  private final Map<Integer, Long> hospitalTransferTokens = new ConcurrentHashMap<>();
  private final Map<Integer, Long> stabilizedUntil = new ConcurrentHashMap<>();
  private final AtomicLong tokenSequence = new AtomicLong();

  public void sendToHospital(Habbo habbo) {
    if (habbo == null) {
      return;
    }
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    int userId = habbo.getHabboInfo().getId();
    int hospitalId = HOSPITAL_ROOM_ID;
    Long stabilizationEnd = stabilizedUntil.get(userId);
    if (stabilizationEnd != null && stabilizationEnd > System.currentTimeMillis()) {
      return;
    }
    stabilizedUntil.remove(userId);
    if (data == null || !data.isDead() || hospitalTransferTokens.containsKey(userId)
        || hospitalId <= 0 || RolePlay.getHospitalService().isHealing(habbo)) {
      return;
    }

    Room hospital = Emulator.getGameEnvironment().getRoomManager().loadRoom(hospitalId);
    if (hospital == null) {
      return;
    }

    long token = tokenSequence.incrementAndGet();
    hospitalTransferTokens.put(userId, token);
    long delay = Math.max(1,
        Emulator.getConfig().getInt("features.hospital.autosend.seconds", 45)) * 1000L;
    Emulator.getThreading().run(() -> run(habbo, data, hospital, token), delay);
  }

  private void run(Habbo habbo, RpAvatar data, Room hospital, long token) {
    if (habbo == null) {
      return;
    }
    int userId = habbo.getHabboInfo().getId();
    if (!hospitalTransferTokens.remove(userId, token) || !data.isDead()) {
      return;
    }
    habbo.goToRoom(hospital.getId());
  }

  public void stabilize(Habbo habbo, int seconds) {
    if (habbo == null) {
      return;
    }
    int userId = habbo.getHabboInfo().getId();
    cancelHospitalTransfer(habbo);
    long end = System.currentTimeMillis() + Math.max(1, seconds) * 1000L;
    stabilizedUntil.put(userId, end);
    Emulator.getThreading().run(() -> {
      if (stabilizedUntil.remove(userId, end)) {
        sendToHospital(habbo);
      }
    }, Math.max(1, seconds) * 1000L);
  }

  public void cancelHospitalTransfer(Habbo habbo) {
    if (habbo == null) {
      return;
    }
    int userId = habbo.getHabboInfo().getId();
    hospitalTransferTokens.remove(userId);
    stabilizedUntil.remove(userId);
  }

  public void onDisconnect(Habbo habbo) {
    cancelHospitalTransfer(habbo);
  }

  public void sendToHospitalAsync(Habbo habbo) {
    CompletableFuture.runAsync(() -> sendToHospital(habbo));
  }
<<<<<<< HEAD
}

=======

  public void cancelPendingHospitalTransfer(Habbo habbo) {
    if (habbo != null) {
      usersBeingSentToHospital.remove(habbo.getHabboInfo().getId());
    }
  }
}
>>>>>>> 39dc96e65 (Modifications collègue ParadiseRP)
