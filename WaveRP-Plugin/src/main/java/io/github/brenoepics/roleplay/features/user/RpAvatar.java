package io.github.brenoepics.roleplay.features.user;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomTileState;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUnitStatus;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserStatusComposer;
import gnu.trove.set.hash.THashSet;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.AnotherStatsComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.UserStatsComposer;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPBed;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.job.JobService;
import io.github.brenoepics.roleplay.features.job.JobsManager;
import io.github.brenoepics.roleplay.features.targetlock.TargetLockService;
import io.github.brenoepics.roleplay.features.user.inventory.Inventory;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.template.CombatTemplates;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Timestamp;
import java.time.Duration;
import java.time.Instant;
import java.util.Date;
import java.util.List;
import java.util.Optional;
import org.jetbrains.annotations.NotNull;
import org.jetbrains.annotations.Nullable;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class RpAvatar {

  private static final Logger LOGGER = LoggerFactory.getLogger(RpAvatar.class);
  public static final String HUNGER_KNOCKOUT_MESSAGE = "%s s'est évanoui de faim.";
  public static final String HUNGER_LOSS_MESSAGE = "Votre faim a diminué de %d. Mangez pour la récupérer !";

  private Habbo habbo;
  private int health;
  private int maxHealth = 100;
  private int maxShield = 25;
  private int shield;
  private boolean dead;
  private boolean passive;

  // New database-based job system
  private JobEntity jobEntity;
  private JobRankEntity jobRankEntity;

  private boolean duty;
  private boolean jailed;
  private long jailTime;
  private double strength;
  private Date lastPayday;
  private Inventory inventory;
  private int organizationId;
  @Nullable
  private AvatarLocation lastPosition;

  private int energy;
  private int maxEnergy = 100;
  private int hunger;
  private int maxHunger = 25;
  private Instant aggressionUntil;

  // Combat stats encapsulated in a dedicated class
  private CombatStats combatStats;

  public RpAvatar(Habbo habbo) {
    this.habbo = habbo;
    this.health = 0;
    this.shield = 0;
    this.energy = 0;
    this.hunger = 25;
    this.aggressionUntil = null;
    this.dead = false;
    this.duty = false;
    this.passive = false;
    this.jailed = false;
    this.jailTime = 0;
    this.strength = 1;
    this.organizationId = 0;
    this.lastPosition = null;
    this.inventory = new Inventory();
    this.combatStats = new CombatStats();

    // Initialize with unemployed job
    this.jobEntity = RolePlay.getJobService().getUnemployedJob();
    this.jobRankEntity = RolePlay.getJobService().getUnemployedRank();
  }

  public static RpAvatar loadUser(Habbo habbo, ResultSet set, Connection connection)
      throws SQLException {
    RpAvatar data = new RpAvatar(habbo);
    data.setHealth(set.getInt("health"));
    data.setShield(set.getInt("shield"));
    data.setEnergy(set.getInt("energy"));
    data.setPassive(false);
    data.dead = set.getInt("dead") == 1 || data.getHealth() < 1;

    // Load job and rank using new database system
    JobService jobService = RolePlay.getJobService();

    // Try to load job by ID first (new system), fall back to name (legacy)
    Integer jobId = set.getObject("job_id", Integer.class);
    if (jobId != null) {
      jobService.getJobById(jobId).ifPresentOrElse(data::setJobEntity,
          () -> data.setJobEntity(jobService.getUnemployedJob()));
    } else {
      data.setJobEntity(jobService.getUnemployedJob());
    }

    // Try to load job rank by ID first (new system), fall back to name (legacy)
    Integer jobRankId = set.getObject("job_rank_id", Integer.class);
    if (jobRankId != null) {
      jobService.getRankById(jobRankId).ifPresentOrElse(data::setJobRankEntity,
          () -> data.setJobRankEntity(jobService.getUnemployedRank()));
    } else {
      data.setJobRankEntity(jobService.getUnemployedRank());
    }

    data.setJailTime(set.getInt("jailtime"));
    Optional<AvatarLocation> location = AvatarLocation.fromString(set.getString("last_pos"));
    location.ifPresent(data::setLastPosition);

    CombatStats combatStats = data.getCombatStats();
    combatStats.setKills(set.getInt("kills"));
    combatStats.setDeaths(set.getInt("deaths"));
    combatStats.setArrests(set.getInt("arrests"));
    combatStats.setKdRatio(set.getDouble("kdratio"));
    combatStats.setPunchesThrown(set.getInt("punches_thrown"));
    combatStats.setPunchesReceived(set.getInt("punches_received"));
    combatStats.setDamageDealt(set.getInt("damage_dealt"));
    combatStats.setDamageReceived(set.getInt("damage_received"));

    data.setHunger(set.getInt("hunger"));
    Timestamp timestamp = set.getTimestamp("aggressionUntil");
    data.setAggressionUntil(timestamp != null ? timestamp.toInstant() : null);

    if (habbo != null) {
      Inventory.createInventory(habbo, connection, data);
    }

    return data;
  }

  public static RpAvatar createUser(Habbo habbo, Connection connection) throws SQLException {
    RpAvatar data = new RpAvatar(habbo);
    data.setHealth(100);
    data.setShield(0);
    data.setEnergy(100);
    data.setHunger(25);
    data.setPassive(true);

    // Use new database job system
    JobService jobService = RolePlay.getJobService();
    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    data.setJobEntity(unemployedJob);
    data.setJobRankEntity(unemployedRank);
    //data.updateLife();

    try (PreparedStatement insertStatement = connection.prepareStatement(
        "INSERT INTO `users_roleplay` (`user_id`, `health`, `shield`, `energy`, `passive`, "
            + "`job_id`, `job_rank_id`, `kills`, `deaths`, `arrests`, `kdratio`, "
            + "`punches_thrown`, `punches_received`, `damage_dealt`, `damage_received`) "
            + "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)")) {

      insertStatement.setInt(1, habbo.getHabboInfo().getId());
      insertStatement.setInt(2, 100);
      insertStatement.setInt(3, 0);
      insertStatement.setInt(4, 100);
      insertStatement.setBoolean(5, true);

      // Use new job IDs instead of names
      insertStatement.setInt(6, unemployedJob.getId());
      insertStatement.setInt(7, unemployedRank.getId());

      // Set default values for combat stats
      CombatStats stats = data.getCombatStats();
      insertStatement.setInt(8, stats.getKills());
      insertStatement.setInt(9, stats.getDeaths());
      insertStatement.setInt(10, stats.getArrests());
      insertStatement.setDouble(11, stats.getKdRatio());
      insertStatement.setInt(12, stats.getPunchesThrown());
      insertStatement.setInt(13, stats.getPunchesReceived());
      insertStatement.setInt(14, stats.getDamageDealt());
      insertStatement.setInt(15, stats.getDamageReceived());

      insertStatement.execute();
    }

    data.setEquippedWeapon(0);
    return data;
  }

  public void takeDamage(int damage, Habbo damager) {
    this.combatStats.recordDamageReceived(damage);

    if (damager != null) {
      RpAvatar damagerAvatar = RolePlay.getAvatarManager().getRpAvatar(damager);
      if (damagerAvatar != null) {
        damagerAvatar.startAggressionTimer();
        damagerAvatar.getCombatStats().recordDamageDealt(damage);
      }
    }

    damageAction(damage, damager);
    updateLife();
  }

  private void damageAction(int damage, Habbo damager) {
    int leftoverDamage = damage;

    if (shield > 0) {
      if (shield >= leftoverDamage) {
        shield -= leftoverDamage;
        leftoverDamage = 0;
      } else {
        leftoverDamage -= shield;
        shield = 0;
      }
    }

    if (leftoverDamage > 0) {
      health -= leftoverDamage;
    }

    if (health <= 0) {
      handleDeath(damager);
    }
  }

  public void updateState() {
    if (isDead()) {
      makeDead();
      return;
    }

    if (isJailed()) {
      makeJailed();
      return;
    }

    if (this.getHealth() <= 0) {
      handleDeath(null);
    }
  }

  public void updateClientData() {
    String user = TargetLockService.getClickedUser(habbo);
    Habbo target = null;
    if (user != null) {
      target = Emulator.getGameEnvironment().getHabboManager().getHabbo(user);
    }
    UserStatsComposer statsComposer = new UserStatsComposer(habbo, target);
    GameClient client = habbo.getClient();
    if (client == null) {
      return;
    }
    client.sendResponse(statsComposer.compose());

    if (client.getHabbo().getHabboInfo().getCurrentRoom() != null) {
      client.getHabbo().getHabboInfo().getCurrentRoom()
          .sendComposer(new AnotherStatsComposer(habbo).compose());
    }
  }

  private void handleDeath(Habbo damager) {
    makeDead();

    this.combatStats.recordDeath();

    if (damager != null) {
      RpAvatar damagerAvatar = RolePlay.getAvatarManager().getRpAvatar(damager);
      if (damagerAvatar != null) {
        damagerAvatar.getCombatStats().recordKill();
      }

      LiveFeed.sendGlobalAlert(LiveFeed.alert(
          CombatTemplates.KILL.format(damager.getHabboInfo().getUsername(),
              habbo.getHabboInfo().getUsername())));
      RolePlay.getWantedManager().handleMurder(damager, habbo);
    } else {
      LiveFeed.sendGlobalAlert(LiveFeed.alert(
          "DECES - " + habbo.getHabboInfo().getUsername() + " vient de mourir."));
    }
  }

  public void makeJailed(Duration duration) {
    Optional<Room> jail = RolePlay.getPrisonService().getJailRoom();
    if (jail.isEmpty()) {
      LOGGER.warn("[Roleplay] Jail room is not found!");
      return;
    }

    long jailEndTime = Emulator.getIntUnixTimestamp() + duration.toSeconds();
    this.setJailTime(jailEndTime);
    this.setJailed(true);

    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        "PRISON - " + habbo.getHabboInfo().getUsername() + " vient d'être emprisonné."));

    habbo.getRoomUnit().setCanWalk(true);
    Room room = habbo.getHabboInfo().getCurrentRoom();

    if (room != null) {
      room.giveEffect(habbo, 0, -1);
    }

    setEquippedWeapon(0);
    if (this.isDuty()) {
      RolePlay.getJobsManager().stopWork(habbo, this);
    } else {
      RolePlay.getJobsManager().removeEmployee(habbo);
    }

    // Persist the sentence before moving rooms so reconnecting cannot bypass it.
    updateDatabase();

    if (room == null || !room.equals(jail.get())) {
      habbo.goToRoom(jail.get().getId());
    }
  }

  public void resetHungry() {
    this.setHunger(this.getMaxHunger());
  }

  public void heal() {
    RolePlay.getDeathHandler().cancelPendingHospitalTransfer(habbo);
    this.setHealth(this.getMaxHealth());
    this.setDead(false);
    this.habbo.getRoomUnit().setCanWalk(true);
    this.updateLife();
    RolePlay.getHospitalService().removeGlow(habbo);

    boolean cantGoUp = isSitLay();
    updateStatus(cantGoUp);
  }

  public void kill() {
    if (isDead()) {
      return;
    }

    handleDeath(null);
    updateLife();
    updateDatabase();
  }

  private void updateStatus(boolean cantGoUp) {
    THashSet<RoomTile> toUpdate = new THashSet<>();
    toUpdate.add(getCurrentLocation());

    if (cantGoUp) {
      RoomTile roomTile = leaveBed();
      if (roomTile != null) {
        this.habbo.getRoomUnit().setGoalLocation(roomTile);
        this.habbo.getRoomUnit().cmdLay = false;
        this.habbo.getRoomUnit().cmdSit = false;
        toUpdate.add(roomTile);
      }
    } else {
      this.habbo.getRoomUnit().clearStatus();
    }

    Emulator.getThreading().run(() -> getRoom().ifPresent(h -> h.updateTiles(toUpdate)), 200);
  }

  private Optional<Room> getRoom() {
    return Optional.ofNullable(this.habbo.getRoomUnit().getRoom());
  }

  private boolean isSitLay() {
    return getCurrentRoom() != null && isSitOrLay();
  }

  private RoomTile leaveBed() {
    Room currentRoom = getCurrentRoom();
    HabboItem topItemAt = currentRoom.getTopItemAt(getCurrentLocation().getX(),
        getCurrentLocation().getY());
    if (topItemAt != null && !topItemAt.getBaseItem().getInteractionType().getType()
        .equals(InteractionRPBed.class)) {
      return null;
    }

    List<RoomTile> around = currentRoom.getLayout().getTilesAround(getCurrentLocation(), 0, false);
    if (around.isEmpty()) {
      return null;
    }

    Optional<RoomTile> tile = getFreeTile(around);
    return tile.orElse(null);
  }

  private static @NotNull Optional<RoomTile> getFreeTile(List<RoomTile> around) {
    for (RoomTile roomTile : around) {
      if (roomTile.state == RoomTileState.OPEN && !roomTile.hasUnits()) {
        return Optional.of(roomTile);
      }
    }
    return Optional.empty();
  }

  private boolean isSitOrLay() {
    return getCurrentLocation().state == RoomTileState.SIT
        || getCurrentLocation().state == RoomTileState.LAY;
  }

  private RoomTile getCurrentLocation() {
    return habbo.getRoomUnit().getCurrentLocation();
  }

  private Room getCurrentRoom() {
    return this.getHabbo().getHabboInfo().getCurrentRoom();
  }

  public void makeJailed() {
    if (this.getJailTime() <= Emulator.getIntUnixTimestamp()) {
      this.setJailed(false);
      return;
    }

    this.setJailed(true);

    habbo.getRoomUnit().setCanWalk(true);
    Room room = habbo.getHabboInfo().getCurrentRoom();

    Optional<Room> jail = RolePlay.getPrisonService().getJailRoom();
    if (jail.isEmpty()) {
      LOGGER.warn("[Roleplay] Jail room is not found!");
      return;
    }

    if (room != null) {
      room.giveEffect(habbo, 0, -1);
    }

    setEquippedWeapon(0);
    if (this.isDuty()) {
      RolePlay.getJobsManager().stopWork(habbo, this);
    } else {
      RolePlay.getJobsManager().removeEmployee(habbo);
    }

    if (!jail.get().equals(room)) {
      habbo.goToRoom(jail.get().getId());
    }
  }

  public void makeDead() {
    health = 0;
    setDead(true);
    Room currentRoom = habbo.getHabboInfo().getCurrentRoom();
    if (RolePlay.getHospitalService().isAtBed(habbo) || RolePlay.getPrisonService()
        .isAtBed(habbo)) {
      return;
    }

    RoomUnit roomUnit = habbo.getRoomUnit();

    roomUnit.cmdLay = true;
    if (currentRoom != null) {
      currentRoom.updateHabbo(habbo);
    }
    roomUnit.cmdSit = true;
    roomUnit.setBodyRotation(getDeadRotation(roomUnit));
    roomUnit.setStatus(RoomUnitStatus.LAY, 0.5 + "");
    if (currentRoom != null) {
      currentRoom.sendComposer(new RoomUserStatusComposer(roomUnit).compose());
    }
    roomUnit.setCanWalk(false);

    RolePlay.getDeathHandler().sendToHospitalAsync(habbo);
  }

  private static RoomUserRotation getDeadRotation(RoomUnit roomUnit) {
    return RoomUserRotation.values()[roomUnit.getBodyRotation().getValue()
        - roomUnit.getBodyRotation().getValue() % 2];
  }

  public void updateLife() {
    if (habbo == null) {
      return;
    }
    this.updateClientData();
  }

  public int getOrganizationId() {
    if (organizationId != 0) {
      return organizationId;
    }

    Integer organization = RolePlay.getOrganizationManager()
        .getUserOrganization(habbo.getHabboInfo().getId());
    if (organization != null) {
      organizationId = organization;
    }

    return organizationId;
  }

  public void updateDatabase() {
    if (this.isDuty() || this.isPassive()) {
      this.setEquippedWeapon(0);
    }
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      try (PreparedStatement statement = connection.prepareStatement(
          "UPDATE `users_roleplay` SET health = ?, shield = ?, energy = ?, passive = ?, "
              + "job_id = ?, job_rank_id = ?, jailtime = ?, equippedweapon = ?, last_pos = ?, "
              + "kills = ?, deaths = ?, arrests = ?, kdratio = ?, punches_thrown = ?, punches_received = ?, "
              + "damage_dealt = ?, damage_received = ?, hunger = ?, aggressionUntil = ?, dead = ? "
              + "WHERE user_id = ?")) {

        statement.setInt(1, this.getHealth());
        statement.setInt(2, this.getShield());
        statement.setInt(3, this.getEnergy());
        statement.setBoolean(4, this.isPassive());

        // New job system - save IDs instead of names
        statement.setObject(5, this.getJobEntity() != null ? this.getJobEntity().getId() : null);
        statement.setObject(6,
            this.getJobRankEntity() != null ? this.getJobRankEntity().getId() : null);

        statement.setLong(7, this.getJailTime());
        Optional<InventorySlot> equippedWeapon = this.getEquippedWeapon();
        statement.setInt(8,
            equippedWeapon.map(inventorySlot -> inventorySlot.getItem().getId()).orElse(0));
        statement.setString(9, buildLastPosition());

        // Set combat stats
        CombatStats stats = this.getCombatStats();
        statement.setInt(10, stats.getKills());
        statement.setInt(11, stats.getDeaths());
        statement.setInt(12, stats.getArrests());
        statement.setDouble(13, stats.getKdRatio());
        statement.setInt(14, stats.getPunchesThrown());
        statement.setInt(15, stats.getPunchesReceived());
        statement.setInt(16, stats.getDamageDealt());
        statement.setInt(17, stats.getDamageReceived());

        statement.setInt(18, this.hunger);
        Timestamp timestamp =
            this.aggressionUntil != null ? new Timestamp(this.aggressionUntil.toEpochMilli())
                : null;
        statement.setTimestamp(19, timestamp);

        statement.setInt(20, this.isDead() ? 1 : 0);
        statement.setInt(21, this.habbo.getHabboInfo().getId());
        statement.execute();
      }

      getInventory().updateDatabase(connection, this.habbo.getHabboInfo().getId());
      LOGGER.info("[NaHabbo Roleplay] Saved user for [{}] {}",
          this.habbo.getHabboInfo().getUsername(), this.habbo.getHabboInfo().getId());
    } catch (SQLException e) {
      LOGGER.error("[NaHabbo Roleplay] Failed to save user for [{}] {}",
          this.habbo.getHabboInfo().getUsername(), this.habbo.getHabboInfo().getId(), e);
    }
  }

  private String buildLastPosition() {
    if (this.habbo == null || this.habbo.getRoomUnit() == null) {
      return "";
    }

    RoomUnit roomUnit = this.habbo.getRoomUnit();
    Room room = roomUnit.getRoom();
    if (room == null) {
      return "";
    }

    return new AvatarLocation(roomUnit.getX(), roomUnit.getY(),
        roomUnit.getBodyRotation().getValue(), room.getId()).toJson().toString();
  }

  public void resetLastPosition() {
    this.lastPosition = null;
  }

  public boolean isAggressive() {
    return aggressionUntil != null && Instant.now().isBefore(aggressionUntil);
  }

  public void startAggressionTimer() {
    this.aggressionUntil = Instant.now()
        .plusSeconds(Emulator.getConfig().getInt("features.aggression.seconds", 600)); // 10 minutes
  }

  /**
   * Executes an action that decreases player energy by a random amount (1-3). Energy will never go
   * below 0.
   */
  public void executeAction() {
    int amount = Emulator.getRandom().nextInt(3) + 1;

    int currentEnergy = getEnergy();

    int newEnergy = Math.max(0, currentEnergy - amount);

    setEnergy(newEnergy);

    if (newEnergy < 20 && newEnergy > 0) {
      habbo.whisper("Votre énergie est inférieure à 20. Reposez-vous ou utilisez un objet énergétique.",
          RoomChatMessageBubbles.ALERT);
    }

    this.updateLife();
  }

  public boolean hasEnergy() {
    return this.getEnergy() > 0;
  }

  public boolean isAtSafeZone() {
    if (this.habbo == null || this.habbo.getHabboInfo() == null
        || this.habbo.getHabboInfo().getCurrentRoom() == null) {
      return false;
    }

    return this.habbo.getHabboInfo().getCurrentRoom().getCategory() == Emulator.getConfig()
        .getInt("features.safezone.category", 11);
  }

  public void decreaseEnergy(int amount) {
    this.energy -= amount;
    if (this.energy < 0) {
      this.energy = 0;
    }
  }

  // Getters
  public Habbo getHabbo() {
    return this.habbo;
  }

  public int getHealth() {
    return this.health;
  }

  public int getMaxHealth() {
    return this.maxHealth;
  }

  public int getShield() {
    return this.shield;
  }

  public boolean isDead() {
    return this.dead;
  }

  public boolean isPassive() {
    return this.passive;
  }

  public JobEntity getJobEntity() {
    return this.jobEntity;
  }

  public JobRankEntity getJobRankEntity() {
    return this.jobRankEntity;
  }

  public boolean isDuty() {
    return this.duty;
  }

  public boolean isJailed() {
    return this.jailed;
  }

  public long getJailTime() {
    return this.jailTime;
  }

  public Optional<InventorySlot> getEquippedWeapon() {
    InventorySlot weaponSlot = inventory.getPrimaryWeaponSlot();
    if (weaponSlot.isEmpty()) {
      return Optional.empty();
    }
    return Optional.of(weaponSlot);
  }

  public void setEquippedWeapon(int enableId) {
    if (enableId == 0) {
      inventory.unEquipWeapon();
      inventory.updateInventory(this.habbo);
      return;
    }

    // Find item by enable ID in inventory slots only (not deposit box)
    RPItem weaponToEquip = null;
    for (InventorySlot slot : inventory.getAllSlots()) {
      if (!slot.isEmpty() && slot.getItem().getEnableId() == enableId) {
        weaponToEquip = slot.getItem();
        break;
      }
    }

    if (weaponToEquip != null && "weapon".equals(weaponToEquip.getInteractionType())) {
      inventory.removeItem(weaponToEquip, 1);
      inventory.equipWeapon(weaponToEquip);
      inventory.updateInventory(this.habbo);
    }
  }

  public double getStrength() {
    return this.strength;
  }

  public Date getLastPayday() {
    return this.lastPayday;
  }

  public Inventory getInventory() {
    return this.inventory;
  }

  public @Nullable AvatarLocation getLastPosition() {
    return this.lastPosition;
  }

  public int getEnergy() {
    return this.energy;
  }

  public int getMaxEnergy() {
    return this.maxEnergy;
  }

  public int getHunger() {
    return this.hunger;
  }

  public int getMaxHunger() {
    return this.maxHunger;
  }

  public Instant getAggressionUntil() {
    return this.aggressionUntil;
  }

  public CombatStats getCombatStats() {
    return this.combatStats;
  }

  // Setters
  public void setHabbo(Habbo habbo) {
    this.habbo = habbo;
  }

  public void setHealth(int health) {
    this.health = health;
  }

  public void setMaxHealth(int maxHealth) {
    this.maxHealth = maxHealth;
  }

  public void setShield(int shield) {
    this.shield = shield;
  }

  public void setDead(boolean dead) {
    if (this.dead == dead) {
      return;
    }
    this.dead = dead;
    if (habbo == null) {
      return;
    }

    if (dead) {
      RolePlay.getJobsManager().stopWork(habbo, this,
          JobsManager.StopReason.DEATH);
      RolePlay.getJobsManager().applyDeathMotto(habbo);
    } else {
      RolePlay.getJobsManager().restoreDeathMotto(habbo);
    }
  }

  public void setPassive(boolean passive) {
    this.passive = passive;
  }

  public void setJobEntity(JobEntity jobEntity) {
    this.jobEntity = jobEntity;
  }

  public void setJobRankEntity(JobRankEntity jobRankEntity) {
    this.jobRankEntity = jobRankEntity;
  }

  public void setDuty(boolean duty) {
    this.duty = duty;
  }

  public void setJailed(boolean jailed) {
    this.jailed = jailed;
  }

  public void setJailTime(long jailTime) {
    this.jailTime = jailTime;
  }

  public void setStrength(double strength) {
    this.strength = strength;
  }

  public void setLastPayday(Date lastPayday) {
    this.lastPayday = lastPayday;
  }

  public void setInventory(Inventory inventory) {
    this.inventory = inventory;
  }

  public void setOrganizationId(int organizationId) {
    this.organizationId = organizationId;
  }

  public void setLastPosition(@Nullable AvatarLocation lastPosition) {
    this.lastPosition = lastPosition;
  }

  public void setEnergy(int energy) {
    this.energy = energy;
  }

  public void setMaxEnergy(int maxEnergy) {
    this.maxEnergy = maxEnergy;
  }

  public void setHunger(int hunger) {
    this.hunger = hunger;
  }

  public void setMaxHunger(int maxHunger) {
    this.maxHunger = maxHunger;
  }

  public void setAggressionUntil(Instant aggressionUntil) {
    this.aggressionUntil = aggressionUntil;
  }

  public void setCombatStats(CombatStats combatStats) {
    this.combatStats = combatStats;
  }

  // Convenience methods for checking job status
  public boolean isUnemployed() {
    return jobEntity == null || jobEntity.isUnemployed();
  }

  public boolean hasJob() {
    return jobEntity != null && !jobEntity.isUnemployed();
  }

  public boolean isManager() {
    return jobRankEntity != null && jobRankEntity.isManager();
  }

  public boolean worksSameJobAs(RpAvatar other) {
    if (this.jobEntity == null || other.jobEntity == null) {
      return false;
    }
    return this.jobEntity.getId() == other.jobEntity.getId() && !this.isUnemployed();
  }

  public boolean canPromote(RpAvatar target) {
    return this.isManager() && this.worksSameJobAs(target)
        && this.jobRankEntity.getLevel() > target.jobRankEntity.getLevel();
  }

  public boolean canDemote(RpAvatar target) {
    return this.isManager() && this.worksSameJobAs(target)
        && this.jobRankEntity.getLevel() > target.jobRankEntity.getLevel();
  }

  public boolean canFire(RpAvatar target) {
    return this.isManager() && this.worksSameJobAs(target)
        && this.jobRankEntity.getLevel() > target.jobRankEntity.getLevel();
  }

  public boolean hasJobPermission(String permission) {
    return jobRankEntity != null && jobRankEntity.hasPermission(permission);
  }

  public boolean hasAnyJobPermission(String... permissions) {
    return jobRankEntity != null && jobRankEntity.hasAnyPermission(permissions);
  }

  public int getMaxShield() {
    return maxShield;
  }

  public void setMaxShield(int maxShield) {
    this.maxShield = maxShield;
  }
}