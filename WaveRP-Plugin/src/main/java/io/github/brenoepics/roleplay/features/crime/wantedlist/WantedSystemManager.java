package io.github.brenoepics.roleplay.features.crime.wantedlist;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.ARREST_TIMEOUT;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.generic.alerts.BubbleAlertComposer;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.template.CombatTemplates;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Timestamp;
import java.sql.Types;
import java.time.Duration;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Optional;
import java.util.Set;
import org.jetbrains.annotations.NotNull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class WantedSystemManager {

  private static final Logger LOGGER = LoggerFactory.getLogger(WantedSystemManager.class);
  public static final String STARS = "%stars%";
  public static final String CRIME = "%crime%";
  public static final String SYSTEM = "System";

  private final Map<Integer, Crime> crimes = new HashMap<>();
  private final Map<Integer, CrimePenalty> penalties = new HashMap<>();
  private final Map<Integer, List<CriminalRecord>> userCriminalRecords = new HashMap<>();

  public WantedSystemManager() {
    loadCrimes();
    loadPenalties();
  }

  public void loadCrimes() {
    crimes.clear();
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "SELECT * FROM crimes"); ResultSet set = statement.executeQuery()) {

      while (set.next()) {
        Crime crime = new Crime(set.getInt("id"), set.getString("name"), set.getInt("stars"),
            set.getBoolean("police_alert"), set.getBoolean("instant_alert"),
            set.getBoolean("is_auto_charge"), set.getString("notes"));
        crimes.put(crime.getId(), crime);
      }
    } catch (SQLException e) {
      LOGGER.error("Error loading crimes", e);
    }
    LOGGER.info("Loaded {} crimes", crimes.size());
  }

  public void loadPenalties() {
    penalties.clear();
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "SELECT * FROM crime_penalties"); ResultSet set = statement.executeQuery()) {

      while (set.next()) {
        CrimePenalty penalty = new CrimePenalty(set.getInt("star_level"), set.getInt("jail_time"),
            set.getInt("fine_amount"));
        penalties.put(penalty.getStarLevel(), penalty);
      }
    } catch (SQLException e) {
      LOGGER.error("Error loading crime penalties", e);
    }
    LOGGER.info("Loaded {} crime penalties", penalties.size());
  }

  public List<CriminalRecord> getUserCriminalRecords(int userId) {
    if (userCriminalRecords.containsKey(userId)) {
      return userCriminalRecords.get(userId);
    }

    List<CriminalRecord> records = new ArrayList<>();
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "SELECT r.*, c.name AS crime_name, "
            + "u1.username AS criminal_username, u2.username AS officer_username "
            + "FROM user_criminal_records r " + "JOIN crimes c ON r.crime_id = c.id "
            + "JOIN users u1 ON r.user_id = u1.id " + "LEFT JOIN users u2 ON r.charged_by = u2.id "
            + "WHERE r.user_id = ? " + "AND r.served_time = 0 AND r.paid_fine = 0 "
            + "ORDER BY r.charged_at DESC")) {

      statement.setInt(1, userId);
      try (ResultSet set = statement.executeQuery()) {
        while (set.next()) {
          CriminalRecord criminalRecord = new CriminalRecord();
          criminalRecord.setId(set.getInt("id"));
          criminalRecord.setUserId(set.getInt("user_id"));
          criminalRecord.setCrimeId(set.getInt("crime_id"));
          criminalRecord.setChargedBy(
              set.getObject("charged_by") != null ? set.getInt("charged_by") : null);
          criminalRecord.setChargedAt(set.getTimestamp("charged_at"));
          criminalRecord.setServedTime(set.getBoolean("served_time"));
          criminalRecord.setPaidFine(set.getBoolean("paid_fine"));

          criminalRecord.setCrime(crimes.get(criminalRecord.getCrimeId()));
          criminalRecord.setCriminalUsername(set.getString("criminal_username"));
          criminalRecord.setOfficerUsername(set.getString("officer_username"));
          criminalRecord.setEndTime(set.getTimestamp("ends_at"));

          if (isCrimeEnded(criminalRecord)) {
            records.add(criminalRecord);
          }
        }
      }
      userCriminalRecords.put(userId, records);
    } catch (SQLException e) {
      LOGGER.error("Error loading criminal records for user {}", userId, e);
    }
    return records;
  }

  private static boolean isCrimeEnded(CriminalRecord criminalRecord) {
    return criminalRecord.getEndTime() != null && criminalRecord.getEndTime()
        .after(new java.util.Date());

  }

  public void addCriminalRecord(int userId, Crime crime, Integer officerId) {
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "INSERT INTO user_criminal_records "
            + "(user_id, crime_id, charged_by, ends_at) VALUES (?, ?, ?, ?)")) {

      statement.setInt(1, userId);
      statement.setInt(2, crime.getId());
      if (officerId != null) {
        statement.setInt(3, officerId);
      } else {
        statement.setNull(3, Types.INTEGER);
      }
      Duration wantedTime = Duration.ofMinutes(
          Emulator.getConfig().getInt("features.wanted.minutes", 7));
      statement.setTimestamp(4,
          new Timestamp(new java.util.Date().getTime() + wantedTime.toMillis()));

      statement.executeUpdate();

      // Clear cache
      userCriminalRecords.remove(userId);
    } catch (SQLException e) {
      LOGGER.error("Error adding criminal record", e);
    }
  }

  public int getUserWantedStars(int userId) {
    return getUserWantedStars(getUserCriminalRecords(userId));
  }

  public int getUserWantedStars(List<CriminalRecord> crimes) {
    int totalStars = 0;
    for (CriminalRecord criminalRecord : crimes) {
      if (!criminalRecord.isServedTime() && !criminalRecord.isPaidFine()) {
        totalStars += criminalRecord.getCrime().getStars();
      }
    }
    return Math.min(totalStars, 5);
  }

  public boolean updateUserCriminalRecord(int userId) {
    List<CriminalRecord> userCrimes = getUserCriminalRecords(userId);
    userCrimes.removeIf(WantedSystemManager::isCrimeChargeable);
    userCriminalRecords.put(userId, userCrimes);
    return userCriminalRecords.get(userId).isEmpty();
  }

  private static boolean isCrimeChargeable(CriminalRecord criminalRecord) {
    return (criminalRecord.getEndTime() != null && !criminalRecord.getEndTime()
        .after(new java.util.Date())) || criminalRecord.isPaidFine()
        || criminalRecord.isServedTime();
  }

  public Optional<Timestamp> getUserWantedEndTimestamp(List<CriminalRecord> crimes) {
    Timestamp wantedEndTimestamp = null;

    for (CriminalRecord crime : crimes) {
      if (wantedEndTimestamp == null || wantedEndTimestamp.before(crime.getEndTime())) {
        wantedEndTimestamp = crime.getEndTime();
      }
    }

    return Optional.ofNullable(wantedEndTimestamp);
  }

  public void autoChargeCrime(Habbo habbo, Crime crime) {
    if (habbo == null) {
      return;
    }

    RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (rpAvatar == null) {
      return;
    }

    addCriminalRecord(habbo.getHabboInfo().getId(), crime, null);

    if (crime.isPoliceAlert()) {
      String message = CombatTemplates.CRIMINAL_ALERT.format(habbo.getHabboInfo().getUsername(),
          SYSTEM).replace(CRIME, crime.getName()).replace(STARS, crime.getDisplayStars());
      LiveFeed.sendGlobalAlert(LiveFeed.alert(message));
    }
  }

  public void chargeCrime(Habbo officer, Habbo criminal, Crime crime) {
    if (officer == null || criminal == null) {
      return;
    }

    addCriminalRecord(criminal.getHabboInfo().getId(), crime, officer.getHabboInfo().getId());

    // Notify players
    criminal.getClient().sendResponse(getCriminalAlert(officer, criminal, crime));
    officer.getClient().sendResponse(getOfficerAlert(officer, criminal, crime));

    // Police alerts
    Optional<JobEntity> police = RolePlay.getJobService().getJobByName("police");
    if (crime.isPoliceAlert() && police.isPresent() && police.get().isActive()) {
      String message = CombatTemplates.OFFICER_CHARGED.format(officer.getHabboInfo().getUsername(),
              criminal.getHabboInfo().getUsername()).replace(CRIME, crime.getName())
          .replace(STARS, crime.getDisplayStars());
      LiveFeed.sendJobAlert(message, police.get(), "911");
    }
  }

  private static @NotNull BubbleAlertComposer getOfficerAlert(Habbo officer, Habbo criminal,
      Crime crime) {
    return LiveFeed.alert(
        CombatTemplates.NOTIFY_OFFICER.format(officer.getHabboInfo().getUsername(),
                criminal.getHabboInfo().getUsername()).replace(CRIME, crime.getName())
            .replace(STARS, crime.getDisplayStars()));
  }

  private static @NotNull BubbleAlertComposer getCriminalAlert(Habbo officer, Habbo criminal,
      Crime crime) {
    return LiveFeed.alert(
        CombatTemplates.NOTIFY_CRIMINAL.format(officer.getHabboInfo().getUsername(),
                criminal.getHabboInfo().getUsername()).replace(CRIME, crime.getName())
            .replace(STARS, crime.getDisplayStars()));
  }

  public Crime getCrimeById(int id) {
    return crimes.get(id);
  }

  public Crime getCrimeByName(String name) {
    return crimes.values().stream().filter(c -> c.getName().equalsIgnoreCase(name)).findFirst()
        .orElse(null);
  }

  public CrimePenalty getPenaltyForStars(int stars) {
    return penalties.get(Math.min(stars, 5));
  }

  public List<Crime> getAllCrimes() {
    return new ArrayList<>(crimes.values());
  }

  public List<Crime> getAutoChargeCrimes() {
    return crimes.values().stream().filter(Crime::isAutoCharge).toList();
  }

  public List<Crime> getManualChargeCrimes() {
    return crimes.values().stream().filter(Crime::isManualCharge).toList();
  }

  public void markTimeServed(int recordId) {
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "UPDATE user_criminal_records SET served_time = TRUE WHERE id = ?")) {
      statement.setInt(1, recordId);
      statement.executeUpdate();

      // Clear relevant cache
      try (PreparedStatement userStatement = connection.prepareStatement(
          "SELECT user_id FROM user_criminal_records WHERE id = ?")) {
        userStatement.setInt(1, recordId);
        try (ResultSet set = userStatement.executeQuery()) {
          if (set.next()) {
            userCriminalRecords.remove(set.getInt("user_id"));
          }
        }
      }
    } catch (SQLException e) {
      LOGGER.error("Error marking time served", e);
    }
  }

  public void markFinePaid(int recordId) {
    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "UPDATE user_criminal_records SET paid_fine = TRUE WHERE id = ?")) {
      statement.setInt(1, recordId);
      statement.executeUpdate();

      // Clear relevant cache
      try (PreparedStatement userStatement = connection.prepareStatement(
          "SELECT user_id FROM user_criminal_records WHERE id = ?")) {
        userStatement.setInt(1, recordId);
        try (ResultSet set = userStatement.executeQuery()) {
          if (set.next()) {
            userCriminalRecords.remove(set.getInt("user_id"));
          }
        }
      }
    } catch (SQLException e) {
      LOGGER.error("Error marking fine paid", e);
    }
  }

  public Map<Habbo, List<CriminalRecord>> getOnlineUsersCrimes() {
    Set<Entry<Integer, Habbo>> habbos = Emulator.getGameEnvironment().getHabboManager()
        .getOnlineHabbos().entrySet();

    HashMap<Habbo, List<CriminalRecord>> crimeList = new HashMap<>();
    for (Entry<Integer, Habbo> entry : habbos) {
      List<CriminalRecord> userCrimes = getUserCriminalRecords(entry.getKey());

      userCrimes.removeIf(cr -> cr.isServedTime() || cr.isPaidFine());
      if (!userCrimes.isEmpty()) {
        crimeList.put(entry.getValue(), userCrimes);
      }
    }

    return crimeList;
  }

  public Map<Integer, List<CriminalRecord>> getCachedWantedList() {
    return userCriminalRecords;
  }

  public boolean determineIfGangHomicide(Habbo killer, Habbo victim) {
    if (killer == null || victim == null) {
      return false;
    }

    RpAvatar killerData = RolePlay.getAvatarManager().getRpAvatar(killer);
    RpAvatar victimData = RolePlay.getAvatarManager().getRpAvatar(victim);

    if (killerData == null || victimData == null) {
      return false;
    }

    return killerData.getOrganizationId() > 0 && victimData.getOrganizationId() > 0;
  }

  public void arrestUser(Habbo police, Habbo criminal) {
    List<CriminalRecord> records = getUserCriminalRecords(criminal.getHabboInfo().getId());
    arrestUser(police, criminal, records, calculateStars(records));
  }

  public void arrestUser(Habbo police, Habbo criminal, List<CriminalRecord> records, int stars) {
    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(criminal);
    if (targetData.isJailed()) {
      police.whisper("This player is already arrested", RoomChatMessageBubbles.ALERT);
      return;
    }

    Optional<Duration> jailTime = calculateJailTime(stars);
    if (jailTime.isEmpty() || records.isEmpty()) {
      police.whisper(
          "This player does not have anything in criminal records or can't be arrested.");
      return;
    }

    StringBuilder crimesList = new StringBuilder();
    for (CriminalRecord crime : records) {
      if (!crime.isServedTime() && !crime.isPaidFine()) {
        crimesList.append(crime.getCrime().getName()).append(", ");
      }
    }

    if (!crimesList.isEmpty()) {
      crimesList.setLength(crimesList.length() - 2);
    }

    executeArrest(police, criminal, crimesList.toString(), targetData, jailTime.get());
    RolePlay.getCommandsCounter().getCoolDown("arrest")
        .addTimeOut(criminal.getHabboInfo().getId(), ARREST_TIMEOUT);
  }

  public static void executeArrest(Habbo police, Habbo criminal, String crimesList,
      RpAvatar criminalData, Duration jailTime) {
    HabboInfo habboInfo = criminal.getHabboInfo();
    police.getHabboInfo().getCurrentRoom()
        .sendComposer(composeMessage(police, habboInfo, crimesList));
    criminalData.makeJailed(jailTime);
    criminalData.getCombatStats().recordArrest();

    RolePlay.getPrisonHandler().sendToJailAsync(criminal);

    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        CombatTemplates.ARREST.format(police.getHabboInfo().getUsername(), habboInfo.getUsername(),
            crimesList)));
  }

  private static ServerMessage composeMessage(Habbo manager, HabboInfo habboInfo, String crimes) {
    return new RoomUserShoutComposer(
        new RoomChatMessage("Arrested " + habboInfo.getUsername() + " for: " + crimes, manager,
            manager, RoomChatMessageBubbles.NORMAL)).compose();
  }

  private Optional<Duration> calculateJailTime(int activeStars) {
    Duration jailTime = null;
    if (activeStars > 0) {
      CrimePenalty penalty = getPenaltyForStars(activeStars);
      if (penalty != null) {
        jailTime = Duration.ofMinutes(penalty.getJailTimeMinutes());
      }
    }
    return Optional.ofNullable(jailTime);
  }


  private int calculateStars(List<CriminalRecord> records) {
    int activeStars = 0;

    for (CriminalRecord criminalRecord : records) {
      if (criminalRecord.isServedTime() || criminalRecord.isPaidFine()) {
        continue;
      }

      activeStars += criminalRecord.getCrime().getStars();

      markTimeServed(criminalRecord.getId());
    }

    activeStars = Math.min(activeStars, 5);
    return activeStars;
  }

  public void handleMurder(Habbo killer, Habbo victim) {
    if (killer == null || victim == null) {
      return;
    }

    Crime crime;
    RpAvatar victimData = RolePlay.getAvatarManager().getRpAvatar(victim);

    if (victimData != null && victimData.isJailed()) {
      crime = getCrimeByName("Execution");
    } else if (determineIfGangHomicide(killer, victim)) {
      crime = getCrimeByName("Gang Homicide");
    } else {
      crime = getCrimeByName("Murder");
    }

    // Apply additional checks
    int killerWantedLevel = getUserWantedStars(killer.getHabboInfo().getId());
    if (killerWantedLevel > 0) {
      // Mass murder - already wanted
      crime = getCrimeByName("Mass Murder");
    }

    // Check if a victim is a cop on duty
    if (victimData != null && victimData.isDuty() && victimData.getJobEntity() != null
        && victimData.getJobEntity().getName().contains("police")) {
      crime = getCrimeByName("Cop Murder");
    }

    autoChargeCrime(killer, crime);
  }

  public void pardonCrimes(Habbo criminal) {
    if (criminal == null) {
      return;
    }

    int userId = criminal.getHabboInfo().getId();

    try (Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); PreparedStatement statement = connection.prepareStatement(
        "UPDATE user_criminal_records SET served_time = TRUE, paid_fine = TRUE "
            + "WHERE user_id = ? AND (served_time = FALSE OR paid_fine = FALSE)")) {

      statement.setInt(1, userId);
      int affectedRows = statement.executeUpdate();
      LOGGER.info("Pardoned {} criminal records for user {}", affectedRows, userId);
    } catch (SQLException e) {
      LOGGER.error("Error pardoning crimes for user {}", userId, e);
    }

    List<CriminalRecord> records = userCriminalRecords.get(userId);
    if (records != null) {
      for (CriminalRecord crime : records) {
        crime.setServedTime(true);
        crime.setPaidFine(true);
      }
    }
  }

}