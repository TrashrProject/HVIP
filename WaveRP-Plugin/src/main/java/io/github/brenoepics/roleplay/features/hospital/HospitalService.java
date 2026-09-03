package io.github.brenoepics.roleplay.features.hospital;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomLayout;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomTileState;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUnitStatus;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.plugin.events.users.HabboAddedToRoomEvent;
import gnu.trove.set.hash.THashSet;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPBed;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Set;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;
import lombok.extern.slf4j.Slf4j;
import org.jetbrains.annotations.Nullable;

@Slf4j
public class HospitalService {

  private final ConcurrentHashMap<Integer, RpAvatar> healingUsers = new ConcurrentHashMap<>();
  private final ConcurrentHashMap<Integer, Long> healingStartedAt = new ConcurrentHashMap<>();
  private final ConcurrentHashMap<Integer, Integer> reservedBeds = new ConcurrentHashMap<>();
  private final ConcurrentHashMap<Integer, Integer> userBedReservations = new ConcurrentHashMap<>();
  private final ConcurrentHashMap<Integer, Integer> lastProgressMinute = new ConcurrentHashMap<>();
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
    return getFirstAvailableBed(hospital, 0);
  }

  private Optional<HabboItem> getFirstAvailableBed(Room hospital, int userId) {
    if (hospital == null) {
      return Optional.empty();
    }

    if (userId > 0) {
      releaseBedReservation(userId);
    }

    List<HabboItem> beds = new ArrayList<>(bedCache.getBeds(hospital));
    beds.sort(Comparator.comparingInt(HabboItem::getId));

    for (HabboItem bed : beds) {
      if (!isUsableBed(hospital, bed) || isBedOccupied(hospital.getLayout(), bed)) {
        continue;
      }

      if (userId <= 0) {
        if (!reservedBeds.containsKey(bed.getId())) {
          return Optional.of(bed);
        }
        continue;
      }

      Integer previous = reservedBeds.putIfAbsent(bed.getId(), userId);
      if (previous == null || previous == userId) {
        userBedReservations.put(userId, bed.getId());
        return Optional.of(bed);
      }
    }

    return Optional.empty();
  }

  private static boolean isUsableBed(Room hospital, HabboItem bed) {
    if (hospital == null || bed == null || bed.getBaseItem() == null
        || !bed.getBaseItem().allowLay()) {
      return false;
    }
    return findPatientTile(hospital, bed).isPresent();
  }

  private static boolean isBedOccupied(RoomLayout layout, HabboItem bed) {
    return bed.getOccupyingTiles(layout).stream().anyMatch(RoomTile::hasUnits);
  }

  public void onEnterHospital(HabboAddedToRoomEvent event, Habbo habbo) {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    Room hospital = event.room;

    if (hospital == null || hospital.getId() != HOSPITAL_ROOM_ID || avatar == null) {
      return;
    }

    alertRegen(habbo, avatar);
    if (!avatar.isDead()) {
      return;
    }

    int userId = habbo.getHabboInfo().getId();
    Optional<HabboItem> bed = getFirstAvailableBed(hospital, userId);

    if (bed.isEmpty()) {
      releaseBedReservation(userId);
      log.warn("Aucun lit d'hopital disponible pour le joueur {}.",
          habbo.getHabboInfo().getUsername());
      // Keep the normal hospital spawn as a safe fallback and preserve the existing recovery.
      startHealingWhenReady(habbo, avatar);
      return;
    }

    HabboItem hospitalBed = bed.get();
    Optional<RoomTile> patientTile = findPatientTile(hospital, hospitalBed);
    if (patientTile.isEmpty()) {
      releaseBedReservation(userId);
      log.warn("Aucun emplacement valide sur le lit d'hopital {} pour le joueur {}.",
          hospitalBed.getId(), habbo.getHabboInfo().getUsername());
      startHealingWhenReady(habbo, avatar);
      return;
    }

    RoomTile tile = patientTile.get();
    // HabboAddedToRoomEvent fires after the RoomUnit exists and belongs to the destination room,
    // but before user positions are sent. Other clients therefore see the patient on the bed
    // immediately, without a second room-entry or delayed teleport system.
    placeDeadPatientOnBed(habbo, avatar, hospital, hospitalBed, tile);
  }

  private void placeDeadPatientOnBed(Habbo habbo, RpAvatar avatar, Room hospital,
      HabboItem bed, RoomTile tile) {
    if (habbo == null || avatar == null || hospital == null || bed == null || tile == null
        || !avatar.isDead() || habbo.getHabboInfo().getCurrentRoom() == null
        || habbo.getHabboInfo().getCurrentRoom().getId() != hospital.getId()) {
      if (habbo != null) {
        releaseBedReservation(habbo.getHabboInfo().getId());
      }
      return;
    }

    int userId = habbo.getHabboInfo().getId();
    Integer reservedBed = userBedReservations.get(userId);
    if (reservedBed == null || reservedBed != bed.getId()) {
      return;
    }

    RoomUnit unit = habbo.getRoomUnit();
    if (unit == null || unit.getRoom() == null || unit.getRoom().getId() != hospital.getId()) {
      releaseBedReservation(userId);
      log.warn("RoomUser indisponible a l'entree de l'hopital pour le joueur {}.",
          habbo.getHabboInfo().getUsername());
      startHealingWhenReady(habbo, avatar);
      return;
    }

    HabboItem topItem = hospital.getTopItemAt(tile.x, tile.y);
    if (topItem == null || topItem.getId() != bed.getId() || !topItem.getBaseItem().allowLay()) {
      releaseBedReservation(userId);
      log.warn("Le lit d'hopital {} n'est plus utilisable pour le joueur {}.", bed.getId(),
          habbo.getHabboInfo().getUsername());
      startHealingWhenReady(habbo, avatar);
      return;
    }

    unit.stopWalking();
    unit.setLocation(tile);
    unit.setZ(bed.getZ());
    unit.setRotation(RoomUserRotation.fromValue(bed.getRotation()));
    unit.cmdSit = false;
    unit.cmdStand = false;
    unit.cmdLay = false;
    unit.removeStatus(RoomUnitStatus.SIT);
    unit.removeStatus(RoomUnitStatus.MOVE);
    unit.setStatus(RoomUnitStatus.LAY, Item.getCurrentHeight(bed) + "");
    unit.setCanWalk(false);
    unit.statusUpdate(true);

    updateTile(hospital, tile);
    hospital.updateHabbo(habbo);
    startHealing(habbo, avatar);

    log.info("[ROLEPLAY] {} a ete place sur le lit d'hopital {} en {},{},{}.",
        habbo.getHabboInfo().getUsername(), bed.getId(), tile.x, tile.y, bed.getZ());
  }

  private void startHealingWhenReady(Habbo habbo, RpAvatar avatar) {
    if (habbo == null || avatar == null || !avatar.isDead()
        || habbo.getHabboInfo().getCurrentRoom() == null
        || habbo.getHabboInfo().getCurrentRoom().getId() != HOSPITAL_ROOM_ID
        || habbo.getRoomUnit() == null) {
      return;
    }
    startHealing(habbo, avatar);
  }

  private static Optional<RoomTile> findPatientTile(Room hospital, HabboItem bed) {
    RoomLayout layout = hospital.getLayout();
    RoomTile anchor = layout.getTile(bed.getX(), bed.getY());
    if (isValidBedTile(anchor, bed, layout)) {
      return Optional.of(anchor);
    }

    return bed.getOccupyingTiles(layout).stream()
        .filter(tile -> isValidBedTile(tile, bed, layout))
        .sorted(Comparator.comparingInt((RoomTile tile) -> tile.x)
            .thenComparingInt(tile -> tile.y))
        .findFirst();
  }

  private static boolean isValidBedTile(RoomTile tile, HabboItem bed, RoomLayout layout) {
    return tile != null && tile.state != RoomTileState.INVALID && tile.state != RoomTileState.BLOCKED
        && bed.getOccupyingTiles(layout).contains(tile);
  }

  private static void alertRegen(Habbo habbo, RpAvatar avatar) {
    if (avatar.getHealth() < avatar.getMaxHealth()) {
      habbo.whisper("Votre sante commence a se regenerer !");
    }
  }

  public static void updateTile(@Nullable Room hospital, @Nullable RoomTile tile) {
    if (hospital == null || tile == null) {
      return;
    }

    hospital.updateTiles(new THashSet<>(Collections.singleton(tile)));
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
    if (habbo == null || avatar == null || habbo.getRoomUnit() == null) {
      return;
    }

    habbo.getRoomUnit().setCanWalk(false);
    habbo.getRoomUnit().statusUpdate(true);
    giveGlow(habbo);

    int userId = habbo.getHabboInfo().getId();
    healingUsers.put(userId, avatar);
    healingStartedAt.putIfAbsent(userId, System.currentTimeMillis());
    lastProgressMinute.putIfAbsent(userId, 0);
  }

  public void giveGlow(Habbo habbo) {
    //Emulator.getThreading()
    //    .run(() -> getHospital().ifPresent(h -> h.giveEffect(habbo, getEffectId(), -1)), 400);
  }

  public void removeGlow(Habbo habbo) {
    if (habbo != null && habbo.getRoomUnit() != null
        && habbo.getRoomUnit().getEffectId() == getEffectId()) {
      //Emulator.getThreading()
      //    .run(() -> getHospital().ifPresent(h -> h.giveEffect(habbo, 0, -1)), 400);
    }
  }

  private static int getEffectId() {
    return Emulator.getConfig().getInt("features.healing.enableid", 23);
  }

  public boolean isHealing(Habbo habbo) {
    return habbo != null && healingUsers.containsKey(habbo.getHabboInfo().getId());
  }

  public void finishHealing(Habbo habbo) {
    if (habbo == null) {
      return;
    }

    int userId = habbo.getHabboInfo().getId();
    healingUsers.remove(userId);
    healingStartedAt.remove(userId);
    lastProgressMinute.remove(userId);
    releaseBedReservation(userId);
  }

  public void onDisconnect(Habbo habbo) {
    finishHealing(habbo);
  }

  public void onLeaveHospital(Habbo habbo) {
    finishHealing(habbo);
  }

  private void releaseBedReservation(int userId) {
    Integer bedId = userBedReservations.remove(userId);
    if (bedId != null) {
      reservedBeds.remove(bedId, userId);
    }
  }

  public boolean markProgressMinute(Habbo habbo, int minute) {
    if (habbo == null || minute <= 0) {
      return false;
    }
    int userId = habbo.getHabboInfo().getId();
    Integer previous = lastProgressMinute.get(userId);
    if (previous != null && previous >= minute) {
      return false;
    }
    lastProgressMinute.put(userId, minute);
    return true;
  }

  public long getHealingStartedAt(Habbo habbo) {
    return healingStartedAt.getOrDefault(habbo.getHabboInfo().getId(),
        System.currentTimeMillis());
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
      if (habboItem == null || habboItem.getBaseItem() == null
          || habboItem.getBaseItem().getInteractionType() == null
          || !habboItem.getBaseItem().allowLay()) {
        return false;
      }

      String configured = Emulator.getConfig()
          .getValue("features.hospital.bed.interactions", "");
      if (configured == null || configured.trim().isEmpty()) {
        // In the hospital room, the furnidata allow_lay flag is the canonical bed marker.
        // An interaction allow-list remains available for installations that need to narrow it.
        return true;
      }
      Set<String> interactionNames = Arrays.stream(configured.split("[,;]"))
          .map(String::trim)
          .filter(value -> !value.isEmpty())
          .collect(Collectors.toCollection(HashSet::new));
      String interactionName = habboItem.getBaseItem().getInteractionType().getName();
      return interactionNames.contains(interactionName)
          || habboItem.getBaseItem().getInteractionType().getType() == InteractionRPBed.class;
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
