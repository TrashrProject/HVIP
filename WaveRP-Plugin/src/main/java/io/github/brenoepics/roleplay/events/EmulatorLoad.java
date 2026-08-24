package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.messages.PacketManager;
import com.eu.habbo.messages.incoming.Incoming;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.emulator.EmulatorLoadedEvent;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.communication.incoming.OverrideRequestRoomLoadEvent;
import io.github.brenoepics.roleplay.communication.incoming.RoomUserWalkEventPlugin;
import io.github.brenoepics.roleplay.communication.incoming.common.RequestUserProfileCustomEvent;
import io.github.brenoepics.roleplay.communication.outgoing.common.GuildRemoveFavoriteEvent;
import io.github.brenoepics.roleplay.communication.outgoing.common.GuildSetFavoriteEvent;
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.RequestWearingBadgesEvent;
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.RequestBankDataEvent; // Added import
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackEvent;
import io.github.brenoepics.roleplay.features.crime.prison.JailTimeRunner;
import io.github.brenoepics.roleplay.features.crime.wantedlist.WantedRunner;
import io.github.brenoepics.roleplay.features.hospital.HealingRunner;
import io.github.brenoepics.roleplay.features.user.HungerRunner;
import io.github.brenoepics.roleplay.utilities.LoadConfig;
import io.github.brenoepics.roleplay.utilities.LoadPlayerCommands;
import java.lang.reflect.Field;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class EmulatorLoad implements EventListener {

  private static final Logger LOGGER = LoggerFactory.getLogger(EmulatorLoad.class);
  private static final int REQUEST_BANK_DATA_PACKET_ID = 3325; // Custom packet id for bank snapshot request

  @EventHandler
  public static void onEmulatorLoaded(EmulatorLoadedEvent event) {
    try {
      LoadPlayerCommands.loadCommands();
      LoadConfig.ILoadConfig();
      PacketManager packetManager = Emulator.getGameServer().getPacketManager();
      Field f = PacketManager.class.getDeclaredField("incoming");
      f.setAccessible(true);
      THashMap<Integer, Class<? extends MessageHandler>> incoming = (THashMap<Integer, Class<? extends MessageHandler>>) f.get(
          packetManager);

      incoming.remove(Incoming.RequestWearingBadgesEvent);
      packetManager.registerHandler(Incoming.RequestWearingBadgesEvent,
          RequestWearingBadgesEvent.class);

      incoming.remove(Incoming.RequestUserProfileEvent);
      incoming.remove(Incoming.GuildSetFavoriteEvent);
      incoming.remove(Incoming.GuildRemoveFavoriteEvent);

      packetManager.registerHandler(Incoming.RequestUserProfileEvent,
          RequestUserProfileCustomEvent.class);
      packetManager.registerHandler(Incoming.GuildSetFavoriteEvent, GuildSetFavoriteEvent.class);
      packetManager.registerHandler(Incoming.GuildRemoveFavoriteEvent,
          GuildRemoveFavoriteEvent.class);

      incoming.remove(Incoming.RoomUserWalkEvent);
      packetManager.registerHandler(Incoming.RoomUserWalkEvent, RoomUserWalkEventPlugin.class);

      // Register custom bank data request packet (no existing constant available)
      incoming.remove(REQUEST_BANK_DATA_PACKET_ID); // ensure clean override if previously registered
      packetManager.registerHandler(REQUEST_BANK_DATA_PACKET_ID, RequestBankDataEvent.class);

      JavascriptCallbackEvent javascriptCallbackEvent = new JavascriptCallbackEvent();
      Emulator.getGameServer().getPacketManager().registerCallable(314, javascriptCallbackEvent);
      startSchedulers();
      LOGGER.info("[NaHabbo RolePlay] Roleplay System {} Loaded",
          EmulatorLoad.class.getPackage().getImplementationVersion());
    } catch (Exception e) {
      LOGGER.error("[NaHabbo RolePlay] Failed to load Roleplay System", e);
    }
  }

  private static void startSchedulers() {
    Emulator.getThreading().run(() -> {
      new WantedRunner().run();
      new HungerRunner().run();
      new HealingRunner().run();
      new JailTimeRunner().run();
    }, 1000);
  }
}
