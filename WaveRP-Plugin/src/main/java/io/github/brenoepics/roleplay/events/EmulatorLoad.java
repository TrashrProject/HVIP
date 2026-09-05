package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.CommandViewRegistry;
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
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.CreateGangEvent;
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.DeleteGangEvent;
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.RequestGangDataEvent;
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.SaveGangBadgeEvent;
import io.github.brenoepics.roleplay.communication.packets.emulator.incoming.SaveGangColorsEvent;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackEvent;
import io.github.brenoepics.roleplay.features.crime.prison.JailTimeRunner;
import io.github.brenoepics.roleplay.features.crime.wantedlist.WantedRunner;
import io.github.brenoepics.roleplay.features.hospital.HealingRunner;
import io.github.brenoepics.roleplay.features.job.AfkWorkRunner;
import io.github.brenoepics.roleplay.features.user.HungerRunner;
import io.github.brenoepics.roleplay.utilities.HabboBrItemCommandRegistrar;
import io.github.brenoepics.roleplay.utilities.LoadConfig;
import io.github.brenoepics.roleplay.utilities.LoadPlayerCommands;
import io.github.brenoepics.roleplay.utilities.RoleplayCommandViewProvider;
import java.lang.reflect.Field;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class EmulatorLoad implements EventListener {

  private static final Logger LOGGER = LoggerFactory.getLogger(EmulatorLoad.class);
  private static final int REQUEST_BANK_DATA_PACKET_ID = 3325; // Custom packet id for bank snapshot request
  private static final int REQUEST_GANG_DATA_PACKET_ID = 6112;
  private static final int CREATE_GANG_PACKET_ID = 6113;
  private static final int SAVE_GANG_COLORS_PACKET_ID = 6114;
  private static final int DELETE_GANG_PACKET_ID = 6119;
  private static final int SAVE_GANG_BADGE_PACKET_ID = 6123;

  @EventHandler
  public static void onEmulatorLoaded(EmulatorLoadedEvent event) {
    try {
      // Register the ParadiseRP item commands first so the permissions created by the registrar
      // are included in the single permissions reload performed at the end of loadCommands().
      // Registering them after loadCommands() left the new permission columns invisible to the
      // in-memory PermissionsManager until a later reload, so :recharger/:pecher/:planter were
      // treated as normal room chat on first startup.
      HabboBrItemCommandRegistrar.register();
      LoadPlayerCommands.loadCommands();
      CommandViewRegistry.setProvider(new RoleplayCommandViewProvider());
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

      incoming.remove(REQUEST_GANG_DATA_PACKET_ID);
      incoming.remove(CREATE_GANG_PACKET_ID);
      incoming.remove(SAVE_GANG_COLORS_PACKET_ID);
      incoming.remove(DELETE_GANG_PACKET_ID);
      incoming.remove(SAVE_GANG_BADGE_PACKET_ID);
      packetManager.registerHandler(REQUEST_GANG_DATA_PACKET_ID, RequestGangDataEvent.class);
      packetManager.registerHandler(CREATE_GANG_PACKET_ID, CreateGangEvent.class);
      packetManager.registerHandler(SAVE_GANG_COLORS_PACKET_ID, SaveGangColorsEvent.class);
      packetManager.registerHandler(DELETE_GANG_PACKET_ID, DeleteGangEvent.class);
      packetManager.registerHandler(SAVE_GANG_BADGE_PACKET_ID, SaveGangBadgeEvent.class);

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
      new AfkWorkRunner().run();
    }, 1000);
  }
}
