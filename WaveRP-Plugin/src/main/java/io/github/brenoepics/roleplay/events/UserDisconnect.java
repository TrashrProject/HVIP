package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.users.UserDisconnectEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.crime.PoliceTaserService;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.job.JobsDelegate;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class UserDisconnect implements EventListener {

  @EventHandler
  public static void onUserDisconnect(UserDisconnectEvent e) {
    Habbo habbo = e.habbo;
    BankComputerSessionManager.disconnect(habbo);
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    data.updateDatabase();
    RolePlay.getEmsService().onDisconnect(habbo);
    RolePlay.getDeathHandler().onDisconnect(habbo);
    RolePlay.getHospitalService().onDisconnect(habbo);
    RolePlay.getOfferManager().getUserOffers(habbo.getHabboInfo().getId()).clear();
    RolePlay.getOfferManager().clearOffers(habbo);
    if (habbo.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      JobsDelegate.resetLook(habbo);
    }

    RolePlay.getJobsManager().onLogout(habbo);
    RolePlay.getEscortManager().stopEscorting(habbo.getHabboInfo().getId());
    RolePlay.getEscortManager().stopEscortingByOfficer(habbo.getHabboInfo().getId());
    RolePlay.getAvatarManager().getCachedData().remove(habbo);
    PoliceHandcuffService.clear(habbo.getHabboInfo().getId());
    PoliceTaserService.clear(habbo.getHabboInfo().getId());
    saveMacro(habbo);
  }

  private static void saveMacro(Habbo habbo) {
    final int macro = (int) habbo.getHabboStats().cache.get("macro");
    try (final Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); final PreparedStatement statement = connection.prepareStatement(
        "UPDATE `users_settings` SET `macro_id` = ? WHERE `user_id` = ? LIMIT 1")) {
      statement.setInt(1, macro);
      statement.setInt(2, habbo.getHabboInfo().getId());
      statement.executeUpdate();
    } catch (SQLException ex) {
      log.error("[MACRO] save error", ex);
    }
  }
}
