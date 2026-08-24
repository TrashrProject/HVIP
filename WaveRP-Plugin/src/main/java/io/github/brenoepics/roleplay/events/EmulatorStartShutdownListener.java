package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.emulator.EmulatorStartShutdownEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobsDelegate;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.concurrent.ConcurrentHashMap;

public class EmulatorStartShutdownListener implements EventListener {

  @EventHandler
  public static void onEmulatorStartShutdown(EmulatorStartShutdownEvent event) {
    ConcurrentHashMap<Integer, Habbo> habboList = Emulator.getGameEnvironment().getHabboManager()
        .getOnlineHabbos();
    for (Habbo habbo : habboList.values()) {
      RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (data == null) {
        continue;
      }
      data.updateDatabase();
      RolePlay.getOfferManager().getUserOffers(habbo.getHabboInfo().getId()).clear();
      RolePlay.getOfferManager().clearOffers(habbo);
      if (habbo.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
        JobsDelegate.resetLook(habbo);
      }

      RolePlay.getAvatarManager().getCachedData().remove(habbo);
    }
  }
}
