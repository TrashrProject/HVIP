package io.github.brenoepics.roleplay.features.job;

import static io.github.brenoepics.roleplay.features.job.JobsDelegate.findLook;
import static io.github.brenoepics.roleplay.features.job.JobsDelegate.getRoomUserShoutComposer;
import static io.github.brenoepics.roleplay.features.job.JobsDelegate.updateLook;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserDataComposer;
import com.eu.habbo.messages.outgoing.users.UserDataComposer;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.template.PassiveTemplates;
import io.github.brenoepics.roleplay.utilities.types.CountDown;
import io.github.brenoepics.roleplay.utilities.types.Look;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import lombok.Getter;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class JobsManager {

  public enum StopReason {
    MANUAL,
    LEFT_WORKPLACE,
    AFK,
    DEATH,
    LOGOUT,
    FORCED,
    JAILED,
    SEND_HOME
  }

  private static final String ORIGINAL_MOTTO_CACHE_KEY = "roleplay.original_motto";
  private static final String DEFAULT_MOTTO = "Citoyen";

  @Getter
  private final JobService jobService;
  @Getter
  private final THashMap<JobEntity, List<Integer>> jobRooms = new THashMap<>();
  @Getter
  private final THashMap<String, List<Look>> jobLooks = new THashMap<>();
  @Getter
  private final CountDown workCountDown = new CountDown();
  @Getter
  private final PaydayTimer paydayTimer = new PaydayTimer();
  @Getter
  private final Map<JobEntity, Set<Habbo>> onDutyEmployees = new HashMap<>();

  public JobsManager() {
    this.jobService = new JobService();

    JobsDelegate jobsDelegate = new JobsDelegate(this);
    jobsDelegate.loadJobsRooms();
    jobsDelegate.loadJobsLooks();
    paydayTimer.init();

    for (JobEntity job : jobService.getAllJobs()) {
      onDutyEmployees.put(job, new HashSet<>());
    }
  }

  public boolean canWorkAtRoom(JobEntity job, int roomId) {
    if (job == null) {
      return false;
    }

    if ("police".equalsIgnoreCase(job.getName())) {
      return true;
    }

    if (jobRooms.containsKey(job)) {
      return jobRooms.get(job).contains(roomId) || jobRooms.get(job).contains(-1);
    }
    return false;
  }

  public boolean startWork(GameClient gameClient, RpAvatar data, Habbo habbo) {
    if (gameClient == null || data == null || habbo == null
        || data.getJobEntity() == null || data.getJobEntity().isUnemployed()
        || data.getJobRankEntity() == null) {
      return false;
    }

    addEmployee(gameClient, data, habbo);
    applyWorkMotto(habbo, data);

    boolean isPolice = "police".equalsIgnoreCase(data.getJobEntity().getName());
    boolean isMedical = "hospital".equalsIgnoreCase(data.getJobEntity().getName());
    RoomChatMessageBubbles bubble = isPolice
        ? RoomChatMessageBubbles.BLUE
        : RoomChatMessageBubbles.NORMAL;
    String displayedJob = isMedical ? "EMS" : data.getJobEntity().getDisplayName();
    String message = "* Commence à travailler en tant que "
        + displayedJob + " "
        + data.getJobRankEntity().getDisplayName() + " *";

    Room room = habbo.getHabboInfo().getCurrentRoom();
    if (room != null) {
      room.sendComposer(getRoomUserShoutComposer(message, habbo, bubble).compose());
    }

    workCountDown.addTimeOut(habbo.getHabboInfo().getId(), JobsDelegate.START_WORK_TIMEOUT);
    data.updateDatabase();
    return true;
  }

  public void stopWork(Habbo habbo, RpAvatar data) {
    stopWork(habbo, data, StopReason.MANUAL);
  }

  public void stopWork(Habbo habbo, RpAvatar data, StopReason reason) {
    if (habbo == null) {
      return;
    }

    BankComputerSessionManager.disconnect(habbo);

    if (data == null) {
      restoreMotto(habbo);
      return;
    }

    if (!data.isDuty() || data.getJobEntity() == null || data.getJobEntity().isUnemployed()) {
      restoreMotto(habbo);
      return;
    }

    data.setDuty(false);
    RolePlay.getEscortManager().stopEscortingByOfficer(habbo.getHabboInfo().getId());
    removeEmployee(habbo);
    restoreMotto(habbo);

    Room room = habbo.getHabboInfo().getCurrentRoom();
    if (room != null && reason != StopReason.LOGOUT) {
      room.sendComposer(getRoomUserShoutComposer("* Arrête de travailler *", habbo).compose());
    }

    workCountDown.addTimeOut(habbo.getHabboInfo().getId(), JobsDelegate.STOP_WORK_TIMEOUT);
    data.updateDatabase();
  }

  public void sendHome(RpAvatar targetData, Habbo target, Habbo habbo, int minutes) {
    BankComputerSessionManager.disconnect(target);
    targetData.setDuty(false);

    JobEntity job = targetData.getJobEntity();
    Set<Habbo> employees = onDutyEmployees.get(job);
    if (employees != null) {
      employees.remove(target);
    }

    removeEmployee(target);
    restoreMotto(target);

    Room managerRoom = habbo.getHabboInfo().getCurrentRoom();
    if (managerRoom != null) {
      managerRoom.sendComposer(getRoomUserShoutComposer(
          "* Renvoie " + target.getHabboInfo().getUsername() + " chez lui pour " + minutes + " minute(s) *",
          habbo).compose());
    }

    Room targetRoom = target.getHabboInfo().getCurrentRoom();
    if (targetRoom != null) {
      targetRoom.sendComposer(getRoomUserShoutComposer("* Arrête de travailler *", target).compose());
    }

    if (target.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      JobsDelegate.resetLook(target);
    }
    workCountDown.addTimeOut(target.getHabboInfo().getId(), minutes * 60);
    targetData.updateDatabase();
  }

  public void quitJob(Habbo habbo, RpAvatar data) {
    BankComputerSessionManager.disconnect(habbo);
    removeEmployee(habbo);
    restoreMotto(habbo);

    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    data.setJobEntity(unemployedJob);
    data.setJobRankEntity(unemployedRank);
    data.setDuty(false);
    removeEmployee(habbo);
    data.updateDatabase();
    habbo.whisper("Vous avez démissionné. Vous êtes maintenant sans emploi.", RoomChatMessageBubbles.ALERT);
  }

  public void promoteUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas à promouvoir.", RoomChatMessageBubbles.ALERT);
      return;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);

    boolean isSameJob = targetData.getJobEntity() != null
        && targetData.getJobEntity().getId() == managerData.getJobEntity().getId()
        && !targetData.getJobEntity().isUnemployed();

    JobRankEntity nextRank = jobService.getRankByJobAndLevel(
        managerData.getJobEntity(),
        targetData.getJobRankEntity().getLevel() + 1
    ).orElse(null);

    String employeeName = habbo.getHabboInfo().getUsername();
    if (nextRank == null || !isSameJob || nextRank.isHigherOrEqualThan(
        managerData.getJobRankEntity())) {
      manager.whisper("Vous ne pouvez pas promouvoir " + employeeName + ".", RoomChatMessageBubbles.ALERT);
      return;
    }

    targetData.setJobRankEntity(nextRank);
    manager.whisper("Vous avez promu " + employeeName + ".", RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    habbo.whisper("Vous avez été promu par " + managerName + ".", RoomChatMessageBubbles.ALERT);
    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        PassiveTemplates.PROMOTE.format(managerName, employeeName, nextRank.getDisplayName())));
  }

  public void demoteUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas à rétrograder.", RoomChatMessageBubbles.ALERT);
      return;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);

    boolean isSameJob = targetData.getJobEntity() != null
        && targetData.getJobEntity().getId() == managerData.getJobEntity().getId()
        && !targetData.getJobEntity().isUnemployed();

    JobRankEntity previousRank = jobService.getRankByJobAndLevel(
        managerData.getJobEntity(),
        targetData.getJobRankEntity().getLevel() - 1
    ).orElse(null);

    String employeeName = habbo.getHabboInfo().getUsername();
    if (previousRank == null || !isSameJob
        || targetData.getJobRankEntity().isHigherOrEqualThan(managerData.getJobRankEntity())) {
      manager.whisper("Vous ne pouvez pas rétrograder " + employeeName + ".", RoomChatMessageBubbles.ALERT);
      return;
    }

    targetData.setJobRankEntity(previousRank);
    manager.whisper("Vous avez rétrogradé " + employeeName + ".", RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    habbo.whisper("Vous avez été rétrogradé par " + managerName + ".", RoomChatMessageBubbles.ALERT);
    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        PassiveTemplates.DEMOTE.format(managerName, employeeName, previousRank.getDisplayName())));
  }

  public void fireUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas à licencier.", RoomChatMessageBubbles.ALERT);
      return;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);

    boolean isSameJob = targetData.getJobEntity() != null
        && targetData.getJobEntity().getId() == managerData.getJobEntity().getId()
        && !targetData.getJobEntity().isUnemployed();

    if (!isSameJob || targetData.getJobRankEntity()
        .isHigherOrEqualThan(managerData.getJobRankEntity())) {
      manager.whisper("Vous ne pouvez pas licencier " + habbo.getHabboInfo().getUsername() + ".",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    removeEmployee(habbo);
    restoreMotto(habbo);

    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    targetData.setJobEntity(unemployedJob);
    targetData.setJobRankEntity(unemployedRank);
    targetData.setDuty(false);
    targetData.updateDatabase();
    manager.whisper("Vous avez licencié " + habbo.getHabboInfo().getUsername() + ".",
        RoomChatMessageBubbles.ALERT);
    habbo.whisper("Vous avez été licencié par " + manager.getHabboInfo().getUsername() + ".",
        RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    String employeeName = habbo.getHabboInfo().getUsername();
    LiveFeed.sendGlobalAlert(
        LiveFeed.alert(PassiveTemplates.FIRE.format(managerName, employeeName)));
  }

  public void onLogout(Habbo habbo) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data != null && data.isDuty()) {
      stopWork(habbo, data, StopReason.LOGOUT);
      return;
    }
    restoreMotto(habbo);
  }

  private void applyWorkMotto(Habbo habbo, RpAvatar data) {
    if (!habbo.getHabboStats().cache.containsKey(ORIGINAL_MOTTO_CACHE_KEY)) {
      String currentMotto = habbo.getHabboInfo().getMotto();
      habbo.getHabboStats().cache.put(ORIGINAL_MOTTO_CACHE_KEY,
          currentMotto == null || currentMotto.trim().isEmpty() ? DEFAULT_MOTTO : currentMotto);
    }

    String displayedJob = "hospital".equalsIgnoreCase(data.getJobEntity().getName())
        ? "EMS" : data.getJobEntity().getDisplayName();
    String workMotto = displayedJob + " " + data.getJobRankEntity().getDisplayName();
    habbo.getHabboInfo().setMotto(workMotto.trim());
    sendMottoUpdate(habbo);
  }

  private void restoreMotto(Habbo habbo) {
    Object originalMotto = habbo.getHabboStats().cache.remove(ORIGINAL_MOTTO_CACHE_KEY);
    String restoredMotto = originalMotto == null ? DEFAULT_MOTTO : originalMotto.toString().trim();
    if (restoredMotto.isEmpty()) {
      restoredMotto = DEFAULT_MOTTO;
    }
    habbo.getHabboInfo().setMotto(restoredMotto);
    sendMottoUpdate(habbo);
  }

  private void sendMottoUpdate(Habbo habbo) {
    if (habbo.getClient() != null) {
      habbo.getClient().sendResponse(new UserDataComposer(habbo));
    }
    Room room = habbo.getHabboInfo().getCurrentRoom();
    if (room != null) {
      room.sendComposer(new RoomUserDataComposer(habbo).compose());
    }
  }

  public void removeEmployee(Habbo habbo) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data != null) {
      JobEntity job = data.getJobEntity();
      Set<Habbo> employees = onDutyEmployees.get(job);
      if (employees != null) {
        employees.remove(habbo);
      }
    }
    RoomUnit unit = habbo.getRoomUnit();
    if (unit != null && unit.getRoom() != null) {
      unit.getRoom().giveEffect(habbo, -1, -1);
    }
  }

  private void addEmployee(GameClient gameClient, RpAvatar data, Habbo habbo) {
    if (getJobLooks().containsKey(data.getJobRankEntity().getName())) {
      Look look = findLook(habbo, getJobLooks().get(data.getJobRankEntity().getName()));
      updateLook(gameClient, habbo, look);
    }

    data.setDuty(true);

    JobEntity job = data.getJobEntity();
    onDutyEmployees.computeIfAbsent(job, ignored -> new HashSet<>()).add(habbo);
  }

  public void reloadJobs() {
    jobService.loadAllJobs();

    onDutyEmployees.clear();
    for (JobEntity job : jobService.getAllJobs()) {
      onDutyEmployees.put(job, new HashSet<>());
    }

    log.info("Jobs reloaded successfully");
  }
}
