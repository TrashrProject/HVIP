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

public class DeathHandler {

  private final Map<Integer, Boolean> usersBeingSentToHospital = new ConcurrentHashMap<>();

  public void sendToHospital(Habbo habbo) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    int hospitalId = HOSPITAL_ROOM_ID;
    if (!data.isDead() || usersBeingSentToHospital.containsKey(habbo.getHabboInfo().getId())
        || hospitalId <= 0 || RolePlay.getHospitalService().isHealing(habbo)) {
      return;
    }

    usersBeingSentToHospital.put(habbo.getHabboInfo().getId(), true);

    Room hospital = Emulator.getGameEnvironment().getRoomManager().loadRoom(hospitalId);
    if (hospital == null) {
      return;
    }

    Emulator.getThreading().run(() -> run(habbo, data, hospital), Emulator.getConfig().getInt("features.hospital.autosend.seconds", 10) * 1000L);
  }

  private void run(Habbo habbo, RpAvatar data, Room hospital) {
    if (habbo == null || !data.isDead() || !usersBeingSentToHospital.containsKey(
        habbo.getHabboInfo().getId())) {
      return;
    }

    habbo.goToRoom(hospital.getId());
    usersBeingSentToHospital.remove(habbo.getHabboInfo().getId());
  }

  public void sendToHospitalAsync(Habbo habbo) {
    CompletableFuture.runAsync(() -> sendToHospital(habbo));
  }
}