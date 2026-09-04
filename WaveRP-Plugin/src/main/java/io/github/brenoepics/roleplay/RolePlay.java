package io.github.brenoepics.roleplay;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.CommandViewRegistry;
import com.eu.habbo.habbohotel.items.ItemInteraction;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.PacketManager;
import com.eu.habbo.messages.incoming.Incoming;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.messages.incoming.rooms.RequestRoomLoadEvent;
import com.eu.habbo.messages.incoming.rooms.users.RoomUserWalkEvent;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.HabboPlugin;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.commands.generic.CommandsCounter;
import io.github.brenoepics.roleplay.events.EmulatorLoad;
import io.github.brenoepics.roleplay.events.EmulatorLoadItemsManagerListener;
import io.github.brenoepics.roleplay.events.EmulatorStartShutdownListener;
import io.github.brenoepics.roleplay.events.RoomLoadedListener;
import io.github.brenoepics.roleplay.events.UserChangeClothing;
import io.github.brenoepics.roleplay.events.UserConnect;
import io.github.brenoepics.roleplay.events.UserDisconnect;
import io.github.brenoepics.roleplay.events.UserEnterRoomListener;
import io.github.brenoepics.roleplay.events.UserTakeStepListener;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.crime.prison.PrisonHandler;
import io.github.brenoepics.roleplay.features.crime.prison.PrisonService;
import io.github.brenoepics.roleplay.features.crime.wantedlist.WantedSystemManager;
import io.github.brenoepics.roleplay.features.escort.EscortManager;
import io.github.brenoepics.roleplay.features.farm.data.FarmManager;
import io.github.brenoepics.roleplay.features.farm.marketplace.MarketplaceManager;
import io.github.brenoepics.roleplay.features.hospital.DeathHandler;
import io.github.brenoepics.roleplay.features.hospital.HospitalService;
import io.github.brenoepics.roleplay.features.hospital.ems.EmsService;
import io.github.brenoepics.roleplay.features.items.ItemManager;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionCoffeeMachine;
import io.github.brenoepics.roleplay.features.job.JobService;
import io.github.brenoepics.roleplay.features.job.JobsManager;
import io.github.brenoepics.roleplay.features.macro.MacroManager;
import io.github.brenoepics.roleplay.features.offer.OfferManager;
import io.github.brenoepics.roleplay.features.organizations.OrganizationManager;
import io.github.brenoepics.roleplay.features.skins.WeaponSkinService;
import io.github.brenoepics.roleplay.features.user.AvatarManager;
import java.lang.reflect.Field;
import lombok.Getter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class RolePlay extends HabboPlugin implements EventListener {

  @Getter
  public static final FarmManager farmManager = new FarmManager();
  private static final Logger LOGGER = LoggerFactory.getLogger(RolePlay.class);
  @Getter
  public static final JobsManager jobsManager = new JobsManager();
  @Getter
  public static final CommandsCounter commandsCounter = new CommandsCounter();
  @Getter
  public static final ItemManager itemManager = new ItemManager();
  @Getter
  public static final OrganizationManager organizationManager = new OrganizationManager();
  @Getter
  public static final OfferManager offerManager = new OfferManager();
  @Getter
  public static final EscortManager escortManager = new EscortManager();
  @Getter
  public static final MacroManager macroManager = new MacroManager();
  @Getter
  private static final WantedSystemManager wantedManager = new WantedSystemManager();
  @Getter
  private static final AvatarManager avatarManager = new AvatarManager();
  @Getter
  private static final DeathHandler deathHandler = new DeathHandler();
  @Getter
  private static final HospitalService hospitalService = new HospitalService();
  @Getter
  private static final EmsService emsService = new EmsService();
  @Getter
  private static final PrisonService prisonService = new PrisonService();
  @Getter
  private static final PrisonHandler prisonHandler = new PrisonHandler();
  @Getter
  private static final MarketplaceManager marketplaceManager = new MarketplaceManager();
  @Getter
  private static final BankManager bankManager = new BankManager();
  @Getter
  private static final WeaponSkinService weaponSkinService = new WeaponSkinService();

  @Override
  public void onEnable() {
    // Register the coffee interaction immediately when the plugin is enabled.
    // ItemManager.load() resolves each items_base.interaction_type to a Java class while loading
    // the base furniture. Registering here guarantees rp_coffee_machine exists before that pass.
    Emulator.getGameEnvironment().getItemManager().addItemInteraction(
        new ItemInteraction("rp_coffee_machine", InteractionCoffeeMachine.class));
    LOGGER.info("[ParadiseRP] Registered item interaction: rp_coffee_machine");

    Emulator.getPluginManager().registerEvents(this, new EmulatorLoad());
    Emulator.getPluginManager().registerEvents(this, new EmulatorStartShutdownListener());
    Emulator.getPluginManager().registerEvents(this, new UserConnect());
    Emulator.getPluginManager().registerEvents(this, new UserDisconnect());
    Emulator.getPluginManager().registerEvents(this, new EmulatorLoadItemsManagerListener());
    Emulator.getPluginManager().registerEvents(this, new UserEnterRoomListener());
    Emulator.getPluginManager().registerEvents(this, new RoomLoadedListener());
    Emulator.getPluginManager().registerEvents(this, new UserTakeStepListener());
    Emulator.getPluginManager().registerEvents(this, new UserChangeClothing());
  }

  @Override
  public void onDisable() {
    try {
      CommandViewRegistry.setProvider(null);
      PacketManager packetManager = Emulator.getGameServer().getPacketManager();
      Field f = PacketManager.class.getDeclaredField("incoming");
      f.setAccessible(true);
      THashMap<Integer, Class<? extends MessageHandler>> incoming = (THashMap<Integer, Class<? extends MessageHandler>>) f.get(
          packetManager);
      incoming.remove(Incoming.RequestRoomLoadEvent);
      Emulator.getGameServer().getPacketManager()
          .registerHandler(Incoming.RequestRoomLoadEvent, RequestRoomLoadEvent.class);

      incoming.remove(3320);
      Emulator.getGameServer().getPacketManager().registerHandler(3320, RoomUserWalkEvent.class);
      farmManager.dispose();
      hospitalService.shutdown();
      LOGGER.info("[NaHabbo RolePlay] Plugin disabled");
    } catch (Exception e) {
      LOGGER.error("[NaHabbo RolePlay] Error while disabling plugin", e);
    }
  }

  public static JobService getJobService() {
    return jobsManager.getJobService();
  }

  @Override
  public boolean hasPermission(Habbo habbo, String s) {
    return false;
  }

  public static void main(String[] args) {
    System.out.println("Don't run this separately");
  }
}
