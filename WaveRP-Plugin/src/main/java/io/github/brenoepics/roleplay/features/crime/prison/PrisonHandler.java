package io.github.brenoepics.roleplay.features.crime.prison;

import static io.github.brenoepics.roleplay.features.crime.prison.PrisonService.JAIL_ROOM_ID;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;

public class PrisonHandler {

  private final Map<Integer, Boolean> usersBeingSentToJail = new ConcurrentHashMap<>();

  public void sendToJail(Habbo habbo) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    int jailId = JAIL_ROOM_ID;

    long currentTime = System.currentTimeMillis();
    if (!data.isJailed() || data.getJailTime() <= currentTime
        || usersBeingSentToJail.containsKey(habbo.getHabboInfo().getId())
        || jailId <= 0 || RolePlay.getPrisonService().isServing(habbo)) {
      return;
    }

    usersBeingSentToJail.put(habbo.getHabboInfo().getId(), true);

    Room jail = Emulator.getGameEnvironment().getRoomManager().loadRoom(jailId);
    if (jail == null) {
      return;
    }

    Emulator.getThreading().run(() -> run(habbo, data, jail), 100);
  }

  private void run(Habbo habbo, RpAvatar data, Room jail) {
    long currentTime = System.currentTimeMillis();
    if (habbo == null || !data.isJailed() || data.getJailTime() <= currentTime
        || !usersBeingSentToJail.containsKey(habbo.getHabboInfo().getId())) {
      return;
    }

    habbo.goToRoom(jail.getId());
    usersBeingSentToJail.remove(habbo.getHabboInfo().getId());
  }

  public void sendToJailAsync(Habbo habbo) {
    CompletableFuture.runAsync(() -> sendToJail(habbo));
  }
}