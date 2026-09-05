package io.github.brenoepics.roleplay.features.job;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboGender;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserDataComposer;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import com.eu.habbo.messages.outgoing.users.UserDataComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Look;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class JobsDelegate {

  private final JobsManager jobsManager;
  public static final Integer START_WORK_TIMEOUT = Emulator.getConfig()
      .getInt("features.start_work_timeout", 5);
  public static final Integer STOP_WORK_TIMEOUT = Emulator.getConfig()
      .getInt("features.stop_work_timeout", 5);

  public JobsDelegate(JobsManager jobsManager) {
    this.jobsManager = jobsManager;
  }

  public void loadJobsLooks() {
    jobsManager.getJobLooks().clear();
    try (final Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); final PreparedStatement statement = connection.prepareStatement(
        "SELECT jr.name, jl.look_m, jl.look_f FROM jobs_looks jl " +
            "JOIN job_ranks jr ON jr.id = jl.job_rank_id " +
            "WHERE jr.active = TRUE")) {
      try (final ResultSet set = statement.executeQuery()) {
        while (set.next()) {
          List<Look> looks = new ArrayList<>();
          looks.add(new Look(HabboGender.M, set.getString("look_m")));
          looks.add(new Look(HabboGender.F, set.getString("look_f")));

          jobsManager.getJobLooks().put(set.getString("name"), looks);
        }
      }
    } catch (SQLException e) {
      log.error("[NaHabbo RolePlay] Error loading job looks", e);
    } finally {
      log.info("[NaHabbo RolePlay] Loaded {} job looks", jobsManager.getJobLooks().size());
    }
  }

  public void loadJobsRooms() {
    jobsManager.getJobRooms().clear();
    try (final Connection connection = Emulator.getDatabase().getDataSource()
        .getConnection(); final PreparedStatement statement = connection.prepareStatement(
        "SELECT j.*, jr.rooms FROM jobs j " +
            "JOIN jobs_rooms jr ON jr.job_id = j.id " +
            "WHERE j.active = TRUE")) {
      try (final ResultSet set = statement.executeQuery()) {
        while (set.next()) {
          JobEntity job = jobsManager.getJobService().getJobById(set.getInt("id")).orElse(null);
          if (job == null) {
            log.warn("Job with ID {} not found in service", set.getInt("id"));
            continue;
          }

          jobsManager.getJobRooms().put(job, parseRooms(set.getString("rooms")));
        }
      }
    } catch (SQLException e) {
      log.error("[NaHabbo RolePlay] Error loading job rooms", e);
    } finally {
      log.info("[NaHabbo RolePlay] Loaded {} job rooms", jobsManager.getJobRooms().size());
    }
  }

  public static RoomUserShoutComposer getRoomUserShoutComposer(String message, Habbo habbo) {
    RoomChatMessageBubbles bubble = RoomChatMessageBubbles.NORMAL;

    if (isPoliceOnDuty(habbo)) {
      bubble = RoomChatMessageBubbles.AMBASSADOR;
    }

    return getRoomUserShoutComposer(message, habbo, bubble);
  }

  public static RoomUserShoutComposer getRoomUserShoutComposer(String message, Habbo habbo,
      RoomChatMessageBubbles bubble) {
    RoomChatMessageBubbles resolvedBubble = bubble;

    if (bubble == RoomChatMessageBubbles.BLUE && isPoliceOnDuty(habbo)) {
      resolvedBubble = RoomChatMessageBubbles.AMBASSADOR;
    }

    String resolvedMessage = resolveRestaurantWorkMessage(message, habbo);
    return new RoomUserShoutComposer(
        new RoomChatMessage(resolvedMessage, habbo, habbo, resolvedBubble));
  }

  private static String resolveRestaurantWorkMessage(String message, Habbo habbo) {
    if (message == null || habbo == null || !message.startsWith("* Commence à travailler en tant que ")) {
      return message;
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data == null || data.getJobEntity() == null || data.getJobRankEntity() == null) {
      return message;
    }

    String jobName = data.getJobEntity().getName();
    if (!"zycroque".equalsIgnoreCase(jobName) && !"tastycrousty".equalsIgnoreCase(jobName)) {
      return message;
    }

    return "* Commence à travailler chez " + data.getJobEntity().getDisplayName()
        + " en tant que " + data.getJobRankEntity().getDisplayName() + " *";
  }

  private static boolean isPoliceOnDuty(Habbo habbo) {
    if (habbo == null) {
      return false;
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    return data != null
        && data.isDuty()
        && data.getJobEntity() != null
        && "police".equalsIgnoreCase(data.getJobEntity().getName());
  }

  public static Look findLook(Habbo habbo, List<Look> figure) {
    Look look = null;
    for (Look l : figure) {
      if (l.getGender() == habbo.getHabboInfo().getGender()) {
        look = l;
        break;
      }
    }
    return look;
  }

  public static void updateLook(GameClient gameClient, Habbo habbo, Look look) {
    if (look != null) {
      String newLook = JobsDelegate.useMannequin(habbo.getHabboInfo().getLook(),
          look.getLookString());
      habbo.getHabboInfo().getHabboStats().cache.put("lastlook", habbo.getHabboInfo().getLook());
      habbo.getHabboInfo().setLook(newLook);
      habbo.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserDataComposer(habbo).compose());
      gameClient.sendResponse(new UserDataComposer(habbo));
    }
  }

  public static String useMannequin(String oldLook, String figure) {
    StringBuilder newFigure = new StringBuilder();

    for (String playerFigurePart : oldLook.split("\\.")) {
      if (!playerFigurePart.startsWith("ch") && !playerFigurePart.startsWith("lg")) {
        newFigure.append(playerFigurePart).append(".");
      }
    }

    String newFigureParts = figure;

    for (String newFigurePart : newFigureParts.split("\\.")) {
      if (newFigurePart.startsWith("hd")) {
        newFigureParts = newFigureParts.replace(newFigurePart, "");
      }
    }

    if (newFigureParts.isEmpty()) {
      return oldLook;
    }

    String newLook = newFigure + newFigureParts;

    if (newLook.length() > 512) {
      return oldLook;
    }

    return newLook;
  }

  public static void resetLook(Habbo habbo) {
    if (habbo == null) {
      return;
    }

    String lastLook = (String) habbo.getHabboInfo().getHabboStats().cache.get("lastlook");
    habbo.getHabboInfo().setLook(lastLook);
    habbo.getHabboInfo().getHabboStats().cache.remove("lastlook");

    if (habbo.getHabboInfo().getCurrentRoom() != null) {
      habbo.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserDataComposer(habbo).compose());
    }
    habbo.getClient().sendResponse(new UserDataComposer(habbo));
  }

  public static List<Integer> parseRooms(String rooms) {
    List<Integer> roomIds = new ArrayList<>();
    String[] split = rooms.split(",");
    for (String s : split) {
      roomIds.add(Integer.parseInt(s));
    }
    return roomIds;
  }
}
