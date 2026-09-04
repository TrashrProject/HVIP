package io.github.brenoepics.roleplay.events;

import static io.github.brenoepics.roleplay.features.user.AvatarManager.sendToSpawn;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.rooms.HotelViewEvent;
import com.eu.habbo.plugin.events.users.UserLoginEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.outgoing.macro.MacroSetComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.macro.Macro;
import io.github.brenoepics.roleplay.features.user.AvatarManager;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Map;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class UserConnect implements EventListener {

  @EventHandler
  public static void onHotelView(HotelViewEvent event) {
    RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(event.habbo);
    sendToSpawn(event.habbo, rpAvatar);
    Emulator.getThreading().run(rpAvatar::updateLife, 10000);
  }

  @EventHandler
  public static void onUserLogin(UserLoginEvent event) {
    Habbo habbo = event.habbo;
    if (habbo == null || habbo.getClient() == null) {
      return;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    // Also repair weapons persisted by older server versions: equipped weapons are
    // session-only and must be returned to the inventory as soon as the user logs in.
    if (avatar.getEquippedWeapon().isPresent()) {
      avatar.getInventory().unEquipWeapon();
      avatar.updateDatabase();
    }
    GameClient loginClient = habbo.getClient();
    AvatarManager.resetSpawnForward(habbo);
    scheduleSpawnRetry(habbo, avatar, loginClient, 1500);
    scheduleSpawnRetry(habbo, avatar, loginClient, 7000);

    Map<Integer, Macro> userMacros = RolePlay.getMacroManager().loadUser(habbo);
    try (final Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); final PreparedStatement statement = connection.prepareStatement(
        "SELECT `macro_id` FROM `users_settings` WHERE `user_id` = ? LIMIT 1")) {
      statement.setInt(1, habbo.getHabboInfo().getId());
      try (final ResultSet set = statement.executeQuery()) {
        if (set.next()) {
          habbo.getHabboStats().cache.put("macro", set.getInt("macro_id"));
        }
      }
    } catch (SQLException err) {
      log.error("[MacroTool]", err);
    }

    avatar.updateLife();
    if (userMacros != null && !userMacros.isEmpty()) {
      Macro macro = getUserMacro(event, userMacros);
      if (macro != null) {
        MacroSetComposer macroSetComposer = new MacroSetComposer(macro);
        Emulator.getThreading().run(
            () -> habbo.getClient().sendResponse(new JavascriptCallbackComposer(macroSetComposer)),
            10000);
      }
    }
  }

  private static void scheduleSpawnRetry(Habbo habbo, RpAvatar avatar, GameClient loginClient,
      long delayMillis) {
    Emulator.getThreading().run(() -> {
      if (habbo.getClient() != loginClient || habbo.getHabboInfo().getCurrentRoom() != null) {
        return;
      }
      AvatarManager.retrySpawnIfRoomless(habbo, avatar);
    }, delayMillis);
  }

  private static Macro getUserMacro(UserLoginEvent event, Map<Integer, Macro> userMacros) {
    Macro macro = userMacros.get(0);
    for (Macro m : userMacros.values()) {
      if (m.getId() == (int) event.habbo.getHabboStats().cache.get("macro")) {
        macro = m;
        break;
      }
    }
    return macro;
  }

}
