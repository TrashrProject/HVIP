
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

  private static final String ORIGINAL_MOTTO_CACHE_KEY = "roleplay.original_motto";

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

    // Initialize lists for all job types
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

  public void startWork(GameClient gameClient, RpAvatar data, Habbo habbo) {
    addEmployee(gameClient, data, habbo);
    applyWorkMotto(habbo, data);
    boolean isPolice = "police".equalsIgnoreCase(data.getJobEntity().getName());
    boolean isMedical = "hospital".equalsIgnoreCase(data.getJobEntity().getName());
    RoomChatMessageBubbles bubble = isPolice
        ? RoomChatMessageBubbles.BLUE
        : RoomChatMessageBubbles.NORMAL;
    String displayedJob = isMedical ? "EMS" : data.getJobEntity().getDisplayName();
    String message = "* Commence \u00e0 travailler en tant que "
        + displayedJob + " "
        + data.getJobRankEntity().getDisplayName() + " *";
    habbo.getHabboInfo().getCurrentRoom()
        .sendComposer(getRoomUserShoutComposer(message, habbo, bubble).compose());
    RolePlay.getJobsManager().getWorkCountDown()
        .addTimeOut(habbo.getHabboInfo().getId(), JobsDelegate.START_WORK_TIMEOUT);
    data.updateDatabase();
  }

  public void stopWork(Habbo habbo, RpAvatar data) {
    BankComputerSessionManager.disconnect(habbo);
    if (!data.isDuty() || data.getJobEntity() == null || data.getJobEntity().isUnemployed()) {
      restoreMotto(habbo);
      return;
    }

    data.setDuty(false);
    RolePlay.getEscortManager().stopEscortingByOfficer(habbo.getHabboInfo().getId());
    removeEmployee(habbo);
    restoreMotto(habbo);
    Room room = habbo.getHabboInfo().getCurrentRoom();

    if (room != null) {
      room.sendComposer(getRoomUserShoutComposer("* Arr\u00eate de travailler *", habbo).compose());
    }

    RolePlay.getJobsManager().getWorkCountDown()
        .addTimeOut(habbo.getHabboInfo().getId(), JobsDelegate.STOP_WORK_TIMEOUT);
    data.updateDatabase();
  }

  public void sendHome(RpAvatar targetData, Habbo target, Habbo habbo, int minutes) {
    BankComputerSessionManager.disconnect(target);
    targetData.setDuty(false);

    JobEntity job = targetData.getJobEntity();
    onDutyEmployees.get(job).remove(target);

    removeEmployee(target);
    habbo.getHabboInfo().getCurrentRoom().sendComposer(getRoomUserShoutComposer(
        "* Renvoie " + target.getHabboInfo().getUsername() + " chez lui pour " + minutes + " minute(s) *",
        habbo).compose());
    target.getHabboInfo().getCurrentRoom()
        .sendComposer(getRoomUserShoutComposer("* Arr\u00eate de travailler *", target).compose());
    if (target.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      JobsDelegate.resetLook(target);
    }
    RolePlay.getJobsManager().getWorkCountDown()
        .addTimeOut(target.getHabboInfo().getId(), minutes * 60);
  }

  public void quitJob(Habbo habbo, RpAvatar data) {
    BankComputerSessionManager.disconnect(habbo);
    removeEmployee(habbo);

    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    data.setJobEntity(unemployedJob);
    data.setJobRankEntity(unemployedRank);
    data.setDuty(false);
    removeEmployee(habbo);
    habbo.whisper("Vous avez d\u00e9missionn\u00e9. Vous \u00eates maintenant sans emploi.", RoomChatMessageBubbles.ALERT);
  }

  public void promoteUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas \u00e0 promouvoir.", RoomChatMessageBubbles.ALERT);
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
    habbo.whisper("Vous avez \u00e9t\u00e9 promu par " + managerName + ".", RoomChatMessageBubbles.ALERT);
    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        PassiveTemplates.PROMOTE.format(managerName, employeeName, nextRank.getDisplayName())));
  }

  public void demoteUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas \u00e0 r\u00e9trograder.", RoomChatMessageBubbles.ALERT);
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
      manager.whisper("Vous ne pouvez pas r\u00e9trograder " + employeeName + ".", RoomChatMessageBubbles.ALERT);
      return;
    }

    targetData.setJobRankEntity(previousRank);
    manager.whisper("Vous avez r\u00e9trograd\u00e9 " + employeeName + ".", RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    habbo.whisper("Vous avez \u00e9t\u00e9 r\u00e9trograd\u00e9 par " + managerName + ".", RoomChatMessageBubbles.ALERT);
    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        PassiveTemplates.DEMOTE.format(managerName, employeeName, previousRank.getDisplayName())));
  }

  public void fireUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("Votre grade ne vous autorise pas \u00e0 licencier.", RoomChatMessageBubbles.ALERT);
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

    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    targetData.setJobEntity(unemployedJob);
    targetData.setJobRankEntity(unemployedRank);
    manager.whisper("Vous avez licenci\u00e9 " + habbo.getHabboInfo().getUsername() + ".",
        RoomChatMessageBubbles.ALERT);
    habbo.whisper("Vous avez \u00e9t\u00e9 licenci\u00e9 par " + manager.getHabboInfo().getUsername() + ".",
        RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    String employeeName = habbo.getHabboInfo().getUsername();
    LiveFeed.sendGlobalAlert(
        LiveFeed.alert(PassiveTemplates.FIRE.format(managerName, employeeName)));
  }

  public void onLogout(Habbo habbo) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    restoreMotto(habbo);
    if (data == null || !data.isDuty()) {
      return;
    }

    JobEntity job = data.getJobEntity();
    onDutyEmployees.get(job).remove(habbo);
    data.setDuty(false);
  }

  private void applyWorkMotto(Habbo habbo, RpAvatar data) {
    if (!habbo.getHabboStats().cache.containsKey(ORIGINAL_MOTTO_CACHE_KEY)) {
      habbo.getHabboStats().cache.put(ORIGINAL_MOTTO_CACHE_KEY,
          habbo.getHabboInfo().getMotto() == null ? "" : habbo.getHabboInfo().getMotto());
    }

    String displayedJob = "hospital".equalsIgnoreCase(data.getJobEntity().getName())
        ? "EMS" : data.getJobEntity().getDisplayName();
    String workMotto = displayedJob + " - "
        + data.getJobRankEntity().getDisplayName();
    int maxLength = 38;
    if (workMotto.length() > maxLength) {
      workMotto = workMotto.substring(0, maxLength);
    }
    habbo.getHabboInfo().setMotto(workMotto);
    sendMottoUpdate(habbo);
  }

  private void restoreMotto(Habbo habbo) {
    Object originalMotto = habbo.getHabboStats().cache.remove(ORIGINAL_MOTTO_CACHE_KEY);
    if (originalMotto == null) return;
    habbo.getHabboInfo().setMotto(originalMotto.toString());
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
    JobEntity job = data.getJobEntity();
    onDutyEmployees.get(job).remove(habbo);
    RoomUnit unit = habbo.getRoomUnit();
    if (unit != null && habbo.getRoomUnit().getRoom() != null) {
      habbo.getRoomUnit().getRoom().giveEffect(habbo, -1, -1);
    }
  }

  private void addEmployee(GameClient gameClient, RpAvatar data, Habbo habbo) {
    if (getJobLooks().containsKey(data.getJobRankEntity().getName())) {
      Look look = findLook(habbo, getJobLooks().get(data.getJobRankEntity().getName()));
      updateLook(gameClient, habbo, look);
    }

    data.setDuty(true);

    JobEntity job = data.getJobEntity();
    onDutyEmployees.get(job).add(habbo);
  }

  public void reloadJobs() {
    jobService.loadAllJobs();

    // Reinitialize employee tracking
    onDutyEmployees.clear();
    for (JobEntity job : jobService.getAllJobs()) {
      onDutyEmployees.put(job, new HashSet<>());
    }

    log.info("Jobs reloaded successfully");
  }
}
