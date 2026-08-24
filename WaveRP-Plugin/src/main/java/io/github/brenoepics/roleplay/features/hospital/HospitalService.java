package io.github.brenoepics.roleplay.features.hospital;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomLayout;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.plugin.events.users.UserEnterRoomEvent;
import gnu.trove.set.hash.THashSet;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPBed;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Collections;
import java.util.HashMap;
import java.util.Optional;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;
import lombok.extern.slf4j.Slf4j;
import org.jetbrains.annotations.Nullable;

@Slf4j
public class HospitalService {

  private final HashMap<Integer, RpAvatar> healingUsers = new HashMap<>();
  private final HospitalBedCache bedCache = new HospitalBedCache();
  private final ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);

  public static int HOSPITAL_ROOM_ID;

  public HospitalService() {
    HOSPITAL_ROOM_ID = Emulator.getConfig().getInt("nahabbo.features.hospital.roomid", 0);
    startCacheInvalidationScheduler();
  }

  private void startCacheInvalidationScheduler() {
    scheduler.scheduleAtFixedRate(() -> {
      bedCache.invalidateCache();
      log.debug("Hospital bed cache invalidated automatically");
    }, 15, 15, TimeUnit.MINUTES);
  }

  public Optional<Room> getHospital() {
    return Optional.ofNullable(
        Emulator.getGameEnvironment().getRoomManager().loadRoom(HOSPITAL_ROOM_ID));
  }

  public Optional<HabboItem> getFirstAvailableBed(Room hospital) {
    if (hospital == null) {
      return Optional.empty();
    }

    THashSet<HabboItem> beds = bedCache.getBeds(hospital);
    return getFirstBed(beds, hospital.getLayout());
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

  public void onEnterHospital(UserEnterRoomEvent event, Habbo habbo) {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);

    Optional<Room> optionalHospital = getHospital();

    Emulator.getThreading()
        .run(() -> optionalHospital.ifPresent(hospital -> alertRegen(habbo, avatar)), 250);

    if (optionalHospital.isEmpty() || !avatar.isDead()) {
      return;
    }

    Room hospital = optionalHospital.get();
    Optional<HabboItem> bed = getFirstAvailableBed(hospital);

    if (bed.isEmpty()) {
      log.warn("Something seems wrong, there is no beds at the hospital.");
      return;
    }

    HabboItem habboItem = bed.get();
    RoomTile tile = getBedPillow(hospital, habboItem);
    event.setDoorTile(tile);
    event.setRotation(RoomUserRotation.fromValue(habboItem.getRotation()));

    updateTile(hospital, tile);
    startHealing(habbo, avatar);
    log.info("[ROLEPLAY] {} has been sent to the hospital bed at {}.", habbo.getHabboInfo().getId(),
        tile.toString());
  }

  private static void alertRegen(Habbo habbo, RpAvatar avatar) {
    if (avatar.getHealth() < avatar.getMaxHealth()) {
      habbo.whisper("Your health has started to regenerate!");
    }
  }

  public static void updateTile(@Nullable Room hospital, @Nullable RoomTile tile) {
    if (hospital == null || tile == null) {
      return;
    }

    hospital.updateTiles(new THashSet<>(Collections.singleton(tile)));
  }

  private static RoomTile getBedPillow(Room hospital, HabboItem habboItem) {
    return hospital.getLayout().getTile(habboItem.getX(), habboItem.getY());
  }

  public boolean isAtBed(Habbo habbo) {
    if (habbo == null || habbo.getRoomUnit() == null
        || habbo.getHabboInfo().getCurrentRoom() == null) {
      return false;
    }

    Room currentRoom = habbo.getHabboInfo().getCurrentRoom();
    if (currentRoom.getId() != HOSPITAL_ROOM_ID) {
      return false;
    }

    RoomTile currentLocation = habbo.getRoomUnit().getCurrentLocation();
    THashSet<HabboItem> beds = bedCache.getBeds(currentRoom);

    return beds.stream().flatMap(bed -> bed.getOccupyingTiles(currentRoom.getLayout()).stream())
        .anyMatch(tile -> tile.equals(currentLocation));
  }

  public void startHealing(Habbo habbo, RpAvatar avatar) {
    habbo.getRoomUnit().setCanWalk(false);
    habbo.getRoomUnit().statusUpdate(true);
    giveGlow(habbo);

    healingUsers.put(habbo.getHabboInfo().getId(), avatar);
  }

  public void giveGlow(Habbo habbo) {
    //Emulator.getThreading()
    //    .run(() -> getHospital().ifPresent(h -> h.giveEffect(habbo, getEffectId(), -1)), 400);
  }

  public void removeGlow(Habbo habbo) {
    if (habbo.getRoomUnit().getEffectId() == getEffectId()) {
      //Emulator.getThreading()
      //    .run(() -> getHospital().ifPresent(h -> h.giveEffect(habbo, 0, -1)), 400);
    }
  }

  private static int getEffectId() {
    return Emulator.getConfig().getInt("features.healing.enableid", 23);
  }

  public boolean isHealing(Habbo habbo) {
    return healingUsers.containsKey(habbo.getHabboInfo().getId());
  }

  public void finishHealing(Habbo habbo) {
    if (!isHealing(habbo)) {
      return;
    }

    this.healingUsers.remove(habbo.getHabboInfo().getId());
  }


  public void invalidateBedCache() {
    bedCache.invalidateCache();
    log.debug("Hospital bed cache manually invalidated");
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

  private static class HospitalBedCache {

    private THashSet<HabboItem> cachedBeds;
    private long lastCacheTime;
    private int cacheHits;
    private int cacheMisses;

    public THashSet<HabboItem> getBeds(Room hospital) {
      if (isCacheValid()) {
        cacheHits++;
        log.debug("Hospital bed cache hit");
        return cachedBeds;
      }

      cacheMisses++;
      log.debug("Hospital bed cache miss - fetching fresh data");

      cachedBeds = hospital.getFloorItems().stream().filter(HospitalBedCache::isABed)
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