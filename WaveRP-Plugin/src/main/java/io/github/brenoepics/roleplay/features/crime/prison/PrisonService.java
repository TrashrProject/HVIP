package io.github.brenoepics.roleplay.features.crime.prison;

import static io.github.brenoepics.roleplay.features.job.JobsDelegate.resetLook;
import static io.github.brenoepics.roleplay.features.job.JobsDelegate.updateLook;
import static io.github.brenoepics.roleplay.utilities.template.SinglePlayerTemplates.RELEASED_JAIL;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomLayout;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboGender;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.plugin.events.users.UserEnterRoomEvent;
import gnu.trove.set.hash.THashSet;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPBed;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.types.Look;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class PrisonService {

  private final HashMap<Integer, RpAvatar> jailedUsers = new HashMap<>();
  private final PrisonBedCache bedCache = new PrisonBedCache();
  private Look jailMaleLook = null;
  private Look jailFemaleLook = null;
  private final ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);

  public static int JAIL_ROOM_ID;
  public static int JAIL_RELEASE_ROOM_ID;

  public PrisonService() {
    JAIL_ROOM_ID = Emulator.getConfig().getInt("nahabbo.features.jail.roomid", 0);
    JAIL_RELEASE_ROOM_ID = Emulator.getConfig().getInt("features.prison.release.roomid", 0);

    String maleLook = Emulator.getConfig().getValue("features.prison.look.m", "");
    if (!maleLook.isEmpty()) {
      this.jailMaleLook = new Look(HabboGender.M, maleLook);
    }

    String femaleLook = Emulator.getConfig().getValue("features.prison.look.f", "");
    if (!femaleLook.isEmpty()) {
      this.jailFemaleLook = new Look(HabboGender.F, femaleLook);
    }

    startCacheInvalidationScheduler();
  }

  private void startCacheInvalidationScheduler() {
    scheduler.scheduleAtFixedRate(() -> {
      bedCache.invalidateCache();
      log.debug("Prison bed cache invalidated automatically");
    }, 15, 15, TimeUnit.MINUTES);
  }

  public Optional<Room> getJailRoom() {
    return Optional.ofNullable(
        Emulator.getGameEnvironment().getRoomManager().loadRoom(JAIL_ROOM_ID));
  }

  public Optional<HabboItem> getFirstAvailableBed(Room jailRoom) {
    if (jailRoom == null) {
      return Optional.empty();
    }

    THashSet<HabboItem> beds = bedCache.getBeds(jailRoom);
    return getFirstBed(beds, jailRoom.getLayout());
  }

  private static Optional<HabboItem> getFirstBed(THashSet<HabboItem> beds, RoomLayout layout) {
    Optional<HabboItem> unoccupiedBed = beds.stream().filter(bed -> !isBedOccupied(layout, bed))
        .findFirst();

    if (unoccupiedBed.isPresent()) {
      return unoccupiedBed;
    }

    return beds.stream().findFirst();
  }

  private static boolean isBedOccupied(RoomLayout layout, HabboItem bed) {
    return bed.getOccupyingTiles(layout).stream().anyMatch(RoomTile::hasUnits);
  }

  public void onEnterJail(UserEnterRoomEvent event, Habbo habbo) {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);

    Optional<Room> optionalJail = getJailRoom();
    if (optionalJail.isEmpty() || !avatar.isJailed()) {

      return;
    }

    Room jailRoom = optionalJail.get();
    Optional<HabboItem> bed = getFirstAvailableBed(jailRoom);

    if (bed.isEmpty()) {
      startJailTime(habbo, avatar);
      log.warn("No roleplay bed found in jail room; using the room entrance.");
      return;
    }

    HabboItem habboItem = bed.get();
    RoomTile tile = getBedTile(jailRoom, habboItem);
    event.setDoorTile(tile);
    event.setRotation(RoomUserRotation.fromValue(habboItem.getRotation()));

    startJailTime(habbo, avatar);
    log.info("[ROLEPLAY] {} has been sent to jail bed at {}.", habbo.getHabboInfo().getId(),
        tile.toString());
  }

  private static RoomTile getBedTile(Room jailRoom, HabboItem habboItem) {
    return jailRoom.getLayout().getTile(habboItem.getX(), habboItem.getY());
  }

  public boolean isAtBed(Habbo habbo) {
    if (habbo == null || habbo.getRoomUnit() == null
        || habbo.getHabboInfo().getCurrentRoom() == null) {
      return false;
    }

    Room currentRoom = habbo.getHabboInfo().getCurrentRoom();
    if (currentRoom.getId() != JAIL_ROOM_ID) {
      return false;
    }

    RoomTile currentLocation = habbo.getRoomUnit().getCurrentLocation();
    THashSet<HabboItem> beds = bedCache.getBeds(currentRoom);

    return beds.stream().flatMap(bed -> bed.getOccupyingTiles(currentRoom.getLayout()).stream())
        .anyMatch(tile -> tile.equals(currentLocation));
  }

  public void startJailTime(Habbo habbo, RpAvatar avatar) {
    habbo.getRoomUnit().setCanWalk(true);
    habbo.getRoomUnit().statusUpdate(true);
    setJailLook(habbo);

    jailedUsers.put(habbo.getHabboInfo().getId(), avatar);
  }

  private void setJailLook(Habbo habbo) {
    if (habbo.getHabboInfo().getGender() == HabboGender.M && jailMaleLook != null) {
      updateLook(habbo.getClient(), habbo, jailMaleLook);
    } else if (habbo.getHabboInfo().getGender() == HabboGender.F && jailFemaleLook != null) {
      updateLook(habbo.getClient(), habbo, jailFemaleLook);
    }
  }

  public boolean isServing(Habbo habbo) {
    return jailedUsers.containsKey(habbo.getHabboInfo().getId());
  }

  public Map<Integer, RpAvatar> getJailedUsers() {
    return jailedUsers;
  }

  public void releaseFromJail(Habbo habbo, RpAvatar avatar) {
    avatar.setJailed(false);
    avatar.setJailTime(0);
    setReleaseLook(habbo);
    LiveFeed.sendGlobalAlert(
        LiveFeed.alert(RELEASED_JAIL.format(habbo.getHabboInfo().getUsername())));

    avatar.updateDatabase();
    habbo.goToRoom(JAIL_RELEASE_ROOM_ID);
    jailedUsers.remove(habbo.getHabboInfo().getId());
    log.debug("[ROLEPLAY] User {} has been released from jail!",
        habbo.getHabboInfo().getUsername());

    habbo.whisper("Vous avez ete libere de prison. Vous etes maintenant libre.");
  }

  private static void setReleaseLook(Habbo habbo) {
    if (habbo.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      resetLook(habbo);
    }
  }

  public void decreaseJailTime(Habbo habbo, RpAvatar avatar, long amount) {
    if (avatar == null || habbo == null) {
      return;
    }

    long newTime = avatar.getJailTime() - amount;
    if (newTime <= Emulator.getIntUnixTimestamp()) {
      releaseFromJail(habbo, avatar);
      return;
    }
    avatar.setJailTime(newTime);
    avatar.updateDatabase();
  }

  public void invalidateBedCache() {
    bedCache.invalidateCache();
    log.debug("Prison bed cache manually invalidated");
  }

  public String getCacheStats() {
    return bedCache.getStats();
  }

  public void shutdown() {
    scheduler.shutdown();
    try {
      if (!scheduler.awaitTermination(5, TimeUnit.SECONDS)) {
        scheduler.shutdownNow();
      }
    } catch (InterruptedException e) {
      scheduler.shutdownNow();
      Thread.currentThread().interrupt();
    }
  }

  private static class PrisonBedCache {

    private THashSet<HabboItem> cachedBeds;
    private long lastCacheTime;
    private int cacheHits;
    private int cacheMisses;

    public THashSet<HabboItem> getBeds(Room jailRoom) {
      if (isCacheValid()) {
        cacheHits++;
        log.debug("Prison bed cache hit");
        return cachedBeds;
      }

      cacheMisses++;
      log.debug("Prison bed cache miss - fetching fresh data");

      cachedBeds = jailRoom.getFloorItems().stream().filter(PrisonBedCache::isABed)
          .collect(Collectors.toCollection(THashSet::new));
      lastCacheTime = System.currentTimeMillis();

      return cachedBeds;
    }

    private static boolean isABed(HabboItem habboItem) {
      return habboItem.getBaseItem().getInteractionType().getType() == InteractionRPBed.class;
    }

    private boolean isCacheValid() {
      return cachedBeds != null
          && (System.currentTimeMillis() - lastCacheTime) < TimeUnit.MINUTES.toMillis(15)
          && !cachedBeds.isEmpty();
    }

    public void invalidateCache() {
      cachedBeds = null;
      lastCacheTime = 0;
    }

    public String getStats() {
      return String.format("Cache Stats - Hits: %d, Misses: %d, Valid: %s", cacheHits, cacheMisses,
          isCacheValid());
    }
  }
}
