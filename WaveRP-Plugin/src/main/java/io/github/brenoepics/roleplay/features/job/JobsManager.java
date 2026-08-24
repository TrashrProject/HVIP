
package io.github.brenoepics.roleplay.features.job;

import static io.github.brenoepics.roleplay.features.job.JobsDelegate.findLook;
import static io.github.brenoepics.roleplay.features.job.JobsDelegate.getRoomUserShoutComposer;
import static io.github.brenoepics.roleplay.features.job.JobsDelegate.updateLook;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.RolePlay;
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
    if (jobRooms.containsKey(job)) {
      return jobRooms.get(job).contains(roomId) || jobRooms.get(job).contains(-1);
    }
    return false;
  }

  public void startWork(GameClient gameClient, RpAvatar data, Habbo habbo) {
    addEmployee(gameClient, data, habbo);
    habbo.getHabboInfo().getCurrentRoom()
        .sendComposer(getRoomUserShoutComposer("starts their shift*", habbo).compose());
    RolePlay.getJobsManager().getWorkCountDown()
        .addTimeOut(habbo.getHabboInfo().getId(), JobsDelegate.START_WORK_TIMEOUT);
    data.updateDatabase();
  }

  public void stopWork(Habbo habbo, RpAvatar data) {
    if (!data.isDuty() || data.getJobEntity() == null || data.getJobEntity().isUnemployed()) {
      return;
    }

    if (habbo.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      JobsDelegate.resetLook(habbo);
    }

    data.setDuty(false);
    removeEmployee(habbo);
    Room room = habbo.getHabboInfo().getCurrentRoom();

    if (room != null) {
      room.sendComposer(getRoomUserShoutComposer("stops their shift*", habbo).compose());
    }

    RolePlay.getJobsManager().getWorkCountDown()
        .addTimeOut(habbo.getHabboInfo().getId(), JobsDelegate.STOP_WORK_TIMEOUT);
    data.updateDatabase();
  }

  public void sendHome(RpAvatar targetData, Habbo target, Habbo habbo, int minutes) {
    targetData.setDuty(false);

    JobEntity job = targetData.getJobEntity();
    onDutyEmployees.get(job).remove(target);

    removeEmployee(target);
    habbo.getHabboInfo().getCurrentRoom().sendComposer(getRoomUserShoutComposer(
        "Sends " + target.getHabboInfo().getUsername() + " home for " + minutes + " minutes*",
        habbo).compose());
    target.getHabboInfo().getCurrentRoom()
        .sendComposer(getRoomUserShoutComposer("stops their shift*", target).compose());
    if (target.getHabboInfo().getHabboStats().cache.containsKey("lastlook")) {
      JobsDelegate.resetLook(target);
    }
    RolePlay.getJobsManager().getWorkCountDown()
        .addTimeOut(target.getHabboInfo().getId(), minutes * 60);
  }

  public void quitJob(Habbo habbo, RpAvatar data) {
    removeEmployee(habbo);

    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    data.setJobEntity(unemployedJob);
    data.setJobRankEntity(unemployedRank);
    data.setDuty(false);
    removeEmployee(habbo);
    habbo.whisper("You have quit your job! You are now unemployed.", RoomChatMessageBubbles.ALERT);
  }

  public void promoteUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("You are not authorized to promote anyone", RoomChatMessageBubbles.ALERT);
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
      manager.whisper("You can't promote " + employeeName, RoomChatMessageBubbles.ALERT);
      return;
    }

    targetData.setJobRankEntity(nextRank);
    manager.whisper("You promoted " + employeeName, RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    habbo.whisper("You were promoted by " + managerName, RoomChatMessageBubbles.ALERT);
    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        PassiveTemplates.PROMOTE.format(managerName, employeeName, nextRank.getDisplayName())));
  }

  public void demoteUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("You are not authorized to demote anyone", RoomChatMessageBubbles.ALERT);
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
      manager.whisper("You can't demote " + employeeName, RoomChatMessageBubbles.ALERT);
      return;
    }

    targetData.setJobRankEntity(previousRank);
    manager.whisper("You demoted " + employeeName, RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    habbo.whisper("You were demoted by " + managerName, RoomChatMessageBubbles.ALERT);
    LiveFeed.sendGlobalAlert(LiveFeed.alert(
        PassiveTemplates.DEMOTE.format(managerName, employeeName, previousRank.getDisplayName())));
  }

  public void fireUser(Habbo manager, RpAvatar managerData, Habbo habbo) {
    if (!managerData.getJobRankEntity().isManager()) {
      manager.whisper("You are not authorized to fire anyone", RoomChatMessageBubbles.ALERT);
      return;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);

    boolean isSameJob = targetData.getJobEntity() != null
        && targetData.getJobEntity().getId() == managerData.getJobEntity().getId()
        && !targetData.getJobEntity().isUnemployed();

    if (!isSameJob || targetData.getJobRankEntity()
        .isHigherOrEqualThan(managerData.getJobRankEntity())) {
      manager.whisper("You can't fire " + habbo.getHabboInfo().getUsername(),
          RoomChatMessageBubbles.ALERT);
      return;
    }

    removeEmployee(habbo);

    JobEntity unemployedJob = jobService.getUnemployedJob();
    JobRankEntity unemployedRank = jobService.getUnemployedRank();

    targetData.setJobEntity(unemployedJob);
    targetData.setJobRankEntity(unemployedRank);
    manager.whisper("You fired " + habbo.getHabboInfo().getUsername(),
        RoomChatMessageBubbles.ALERT);
    habbo.whisper("You were fired by " + manager.getHabboInfo().getUsername(),
        RoomChatMessageBubbles.ALERT);
    String managerName = manager.getHabboInfo().getUsername();
    String employeeName = habbo.getHabboInfo().getUsername();
    LiveFeed.sendGlobalAlert(
        LiveFeed.alert(PassiveTemplates.FIRE.format(managerName, employeeName)));
  }

  public void onLogout(Habbo habbo) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data == null || !data.isDuty()) {
      return;
    }

    JobEntity job = data.getJobEntity();
    onDutyEmployees.get(job).remove(habbo);
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
    if (RolePlay.getJobsManager().getJobLooks().containsKey(data.getJobRankEntity().getName())) {
      List<Look> figure = RolePlay.getJobsManager().getJobLooks()
          .get(data.getJobRankEntity().getName());
      Look look = findLook(habbo, figure);
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