package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobEntity;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.job.JobService;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Optional;

public class SuperHireCommand extends Command {

  private static final int MINIMUM_STAFF_RANK = 5;
  private static final int MAXIMUM_STAFF_RANK = 9;

  public SuperHireCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();
    int staffRank = staff.getHabboInfo().getRank().getId();
    if (staffRank < MINIMUM_STAFF_RANK || staffRank > MAXIMUM_STAFF_RANK) {
      sendNoPermission(staff);
      return true;
    }

    if (params.length < 4) {
      staff.whisper(":superhire <pseudo> <m\u00e9tier|id> <grade>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = params[1];
    String rankInput = params[params.length - 1];
    StringBuilder jobInputBuilder = new StringBuilder();
    for (int i = 2; i < params.length - 1; i++) {
      if (jobInputBuilder.length() > 0) {
        jobInputBuilder.append(' ');
      }
      jobInputBuilder.append(params[i]);
    }

    JobService jobService = RolePlay.getJobService();
    JobEntity job = findJob(jobService, jobInputBuilder.toString()).orElse(null);
    if (job == null || !job.isActive() || job.isUnemployed()) {
      staff.whisper("Ce m\u00e9tier n'existe pas.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    JobRankEntity jobRank = findRank(jobService, job, rankInput).orElse(null);
    if (jobRank == null || !jobRank.isActive()) {
      staff.whisper("Ce grade n'existe pas pour ce m\u00e9tier.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo onlineTarget = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);
    if (onlineTarget != null) {
      if (!updateOnlineTarget(onlineTarget, job, jobRank)) {
        staff.whisper("Impossible de mettre \u00e0 jour ce joueur.", RoomChatMessageBubbles.ALERT);
        return true;
      }
      username = onlineTarget.getHabboInfo().getUsername();
      onlineTarget.whisper("Votre m\u00e9tier est maintenant " + job.getDisplayName() + " ("
          + jobRank.getDisplayName() + ").", RoomChatMessageBubbles.ALERT);
    } else {
      String storedUsername = updateOfflineTarget(username, job, jobRank);
      if (storedUsername == null) {
        staff.whisper("Ce joueur est introuvable.", RoomChatMessageBubbles.ALERT);
        return true;
      }
      username = storedUsername;
    }

    staff.whisper("Le joueur " + username + " a \u00e9t\u00e9 recrut\u00e9 dans "
        + job.getDisplayName() + " au grade " + jobRank.getLevel() + " ("
        + jobRank.getDisplayName() + ").", RoomChatMessageBubbles.NORMAL);
    return true;
  }

  public boolean handlePermissionDenied(GameClient gameClient, String[] params) {
    sendNoPermission(gameClient.getHabbo());
    return true;
  }

  private static void sendNoPermission(Habbo habbo) {
    habbo.whisper("Vous n'avez pas la permission d'utiliser cette commande.",
        RoomChatMessageBubbles.ALERT);
  }

  private static Optional<JobEntity> findJob(JobService service, String input) {
    try {
      int jobId = Integer.parseInt(input);
      return service.getJobById(jobId);
    } catch (NumberFormatException ignored) {
      Optional<JobEntity> byName = service.getJobByName(input);
      if (byName.isPresent()) {
        return byName;
      }
      return service.getAllJobs().stream()
          .filter(job -> job.getDisplayName().equalsIgnoreCase(input))
          .findFirst();
    }
  }

  private static Optional<JobRankEntity> findRank(JobService service, JobEntity job, String input) {
    try {
      int rankNumber = Integer.parseInt(input);
      Optional<JobRankEntity> byLevel = service.getRankByJobAndLevel(job, rankNumber);
      if (byLevel.isPresent()) {
        return byLevel;
      }
      return service.getRankById(rankNumber)
          .filter(rank -> rank.getJobId() == job.getId());
    } catch (NumberFormatException ignored) {
      Optional<JobRankEntity> byName = service.getRankByJobAndName(job, input);
      if (byName.isPresent()) {
        return byName;
      }
      return job.getRanks().stream()
          .filter(rank -> rank.getDisplayName().equalsIgnoreCase(input))
          .findFirst();
    }
  }

  private static boolean updateOnlineTarget(Habbo target, JobEntity job, JobRankEntity rank) {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (avatar == null) {
      return false;
    }

    if (avatar.isDuty()) {
      RolePlay.getJobsManager().stopWork(target, avatar);
    }
    avatar.setDuty(false);
    avatar.setJobEntity(job);
    avatar.setJobRankEntity(rank);
    avatar.updateDatabase();
    avatar.updateClientData();
    return true;
  }

  private static String updateOfflineTarget(String username, JobEntity job, JobRankEntity rank) {
    String findUser = "SELECT `id`, `username` FROM `users` WHERE `username` = ? LIMIT 1";
    String saveJob = "INSERT INTO `users_roleplay` (`user_id`, `job_id`, `job_rank_id`) "
        + "VALUES (?, ?, ?) ON DUPLICATE KEY UPDATE `job_id` = VALUES(`job_id`), "
        + "`job_rank_id` = VALUES(`job_rank_id`)";

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection()) {
      connection.setAutoCommit(false);
      try {
        int userId;
        String storedUsername;
        try (PreparedStatement statement = connection.prepareStatement(findUser)) {
          statement.setString(1, username);
          try (ResultSet result = statement.executeQuery()) {
            if (!result.next()) {
              connection.rollback();
              return null;
            }
            userId = result.getInt("id");
            storedUsername = result.getString("username");
          }
        }

        try (PreparedStatement statement = connection.prepareStatement(saveJob)) {
          statement.setInt(1, userId);
          statement.setInt(2, job.getId());
          statement.setInt(3, rank.getId());
          statement.executeUpdate();
        }
        connection.commit();
        return storedUsername;
      } catch (SQLException exception) {
        connection.rollback();
        throw exception;
      } finally {
        connection.setAutoCommit(true);
      }
    } catch (SQLException exception) {
      return null;
    }
  }
}
