package io.github.brenoepics.roleplay.communication.outgoing.common;


import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.guilds.Guild;
import com.eu.habbo.habbohotel.messenger.Messenger;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.eu.habbo.messages.outgoing.Outgoing;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class UserProfileCustomComposer extends MessageComposer {

  private final HabboInfo habboInfo;
  private Habbo habbo;
  private GameClient viewer;
  private final RpAvatar rpAvatar;

  public UserProfileCustomComposer(HabboInfo habboInfo, GameClient viewer, RpAvatar rpAvatar) {
    this.habboInfo = habboInfo;
    this.viewer = viewer;
    this.rpAvatar = rpAvatar;
  }

  public UserProfileCustomComposer(Habbo habbo, GameClient viewer, RpAvatar rpAvatar) {
    this.habbo = habbo;
    this.habboInfo = habbo.getHabboInfo();
    this.viewer = viewer;
    this.rpAvatar = rpAvatar;
  }

  @Override
  protected ServerMessage composeInternal() {
    if (this.habboInfo == null) {
      return null;
    }

    this.response.init(Outgoing.UserProfileComposer);

    this.response.appendInt(this.habboInfo.getId());
    this.response.appendString(this.habboInfo.getUsername());
    this.response.appendString(this.habboInfo.getLook());
    this.response.appendString(this.habboInfo.getMotto());
    this.response.appendString(new SimpleDateFormat("dd-MM-yyyy").format(
        new Date(this.habboInfo.getAccountCreated() * 1000L)));

    int achievementScore = 0;
    if (this.habbo != null) {
      achievementScore = this.habbo.getHabboStats().getAchievementScore();
    } else {
      try (Connection connection = Emulator.getDatabase().getDataSource()
          .getConnection(); PreparedStatement statement = connection.prepareStatement(
          "SELECT achievement_score FROM users_settings WHERE user_id = ? LIMIT 1")) {
        statement.setInt(1, this.habboInfo.getId());
        try (ResultSet set = statement.executeQuery()) {
          if (set.next()) {
            achievementScore = set.getInt("achievement_score");
          }
        }
      } catch (SQLException e) {
        log.error("Caught SQL exception", e);
      }
    }
    this.response.appendInt(achievementScore);
    this.response.appendInt(Messenger.getFriendCount(this.habboInfo.getId()));
    this.response.appendBoolean(this.viewer.getHabbo().getMessenger().getFriends()
        .containsKey(this.habboInfo.getId())); //Friend
    this.response.appendBoolean(
        Messenger.friendRequested(this.viewer.getHabbo().getHabboInfo().getId(),
            this.habboInfo.getId())); //Friend Request Send
    this.response.appendBoolean(this.habboInfo.isOnline());

    List<Guild> guilds = new ArrayList<>();
    if (this.habbo != null) {
      List<Integer> toRemove = new ArrayList<>();
      for (int index = this.habbo.getHabboStats().guilds.size(); index > 0; index--) {
        int i = this.habbo.getHabboStats().guilds.get(index - 1);
        if (i == 0) {
          continue;
        }

        Guild guild = Emulator.getGameEnvironment().getGuildManager().getGuild(i);

        if (guild != null) {
          guilds.add(guild);
        } else {
          toRemove.add(i);
        }
      }

      for (int i : toRemove) {
        this.habbo.getHabboStats().removeGuild(i);
      }
    } else {
      guilds = Emulator.getGameEnvironment().getGuildManager().getGuilds(this.habboInfo.getId());
    }

    this.response.appendInt(guilds.size());
    for (Guild guild : guilds) {
      this.response.appendInt(guild.getId());
      this.response.appendString(guild.getName());
      this.response.appendString(guild.getBadge());
      this.response.appendString(Emulator.getGameEnvironment().getGuildManager()
          .getSymbolColor(guild.getColorOne()).valueA);
      this.response.appendString(Emulator.getGameEnvironment().getGuildManager()
          .getSymbolColor(guild.getColorTwo()).valueA);
      this.response.appendBoolean(
          this.habbo != null && guild.getId() == this.habbo.getHabboStats().guild);
      this.response.appendInt(guild.getOwnerId());
      this.response.appendBoolean(guild.getOwnerId() == this.habboInfo.getId());
    }

    this.response.appendInt(
        Emulator.getIntUnixTimestamp() - this.habboInfo.getLastOnline()); //Secs ago.
    this.response.appendBoolean(true);

    boolean hasRpAvatar = rpAvatar != null;
    this.response.appendBoolean(hasRpAvatar && rpAvatar.isDuty());
    this.response.appendString(hasRpAvatar && rpAvatar.getJobEntity() != null
        ? rpAvatar.getJobEntity().getDisplayName() : "Sans emploi");
    this.response.appendString(hasRpAvatar && rpAvatar.getJobRankEntity() != null
        ? rpAvatar.getJobRankEntity().getDisplayName() : "Sans grade");
    Organization organization = hasRpAvatar ? RolePlay.getOrganizationManager()
        .getOrganization(rpAvatar.getOrganizationId()) : null;

    this.response.appendString(organization == null ? "Aucun gang" : organization.getName());

    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getKills() : 0);
    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getDeaths() : 0);
    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getArrests() : 0);
    this.response.appendDouble(hasRpAvatar ? rpAvatar.getCombatStats().getKdRatio() : 0);
    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getPunchesThrown() : 0);
    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getPunchesReceived() : 0);
    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getDamageDealt() : 0);
    this.response.appendInt(hasRpAvatar ? rpAvatar.getCombatStats().getDamageReceived() : 0);

    this.response.appendInt(0); // Attribute points are not implemented server-side yet.
    this.response.appendInt(hasRpAvatar ? (int) Math.round(rpAvatar.getStrength()) : 1);
    this.response.appendInt(0); // Knowledge is not implemented server-side yet.

    List<ProfileSkill> skills = loadEquippedSkills(this.habboInfo.getId());
    this.response.appendInt(skills.size());
    for (ProfileSkill skill : skills) {
      this.response.appendString(skill.iconLink);
      this.response.appendInt(skill.id);
      this.response.appendInt(skill.level);
    }

    this.response.appendInt(1);
    this.response.appendInt(0);
    this.response.appendInt(100);

    ProfileJobGroup jobGroup = hasRpAvatar && rpAvatar.getJobEntity() != null
        ? loadJobGroup(this.habboInfo.getId(), rpAvatar.getJobEntity().getName(),
            rpAvatar.getJobEntity().getDisplayName()) : null;
    this.response.appendInt(jobGroup == null ? 0 : jobGroup.id);
    if (jobGroup != null) {
      this.response.appendString(jobGroup.name);
      this.response.appendString(jobGroup.badge);
      this.response.appendInt(jobGroup.kind);
      this.response.appendInt(jobGroup.weeklyShifts);
      this.response.appendInt(jobGroup.totalShifts);
    }

    return this.response;
  }

  private static List<ProfileSkill> loadEquippedSkills(int userId) {
    List<ProfileSkill> skills = new ArrayList<>();
    String query = "SELECT skill.id, skill.badge_code, COALESCE(MAX(skill_level.level), 0) AS level "
        + "FROM user_rp_skills AS user_skill "
        + "INNER JOIN rp_skills AS skill ON skill.id = user_skill.skill_id "
        + "LEFT JOIN rp_skills_levels AS skill_level ON skill_level.skill_id = skill.id "
        + "AND user_skill.progress >= skill_level.required_progress "
        + "WHERE user_skill.user_id = ? AND user_skill.equipped = 1 "
        + "GROUP BY skill.id, skill.badge_code ORDER BY user_skill.id LIMIT 4";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(query)) {
      statement.setInt(1, userId);
      try (ResultSet set = statement.executeQuery()) {
        while (set.next()) {
          skills.add(new ProfileSkill(set.getInt("id"), set.getInt("level"),
              set.getString("badge_code") == null ? "" : set.getString("badge_code")));
        }
      }
    } catch (SQLException e) {
      log.error("Unable to load profile skills for user {}", userId, e);
    }
    return skills;
  }

  private static ProfileJobGroup loadJobGroup(int userId, String jobName,
      String jobDisplayName) {
    String query = "SELECT group_data.id, group_data.name, group_data.badge, "
        + "COALESCE(group_data.group_type, 0) AS group_type, "
        + "SUM(CASE WHEN shift_log.shift_finished >= ? THEN 1 ELSE 0 END) AS weekly_shifts, "
        + "COUNT(shift_log.id) AS total_shifts FROM groups AS group_data "
        + "LEFT JOIN rp_shift_logs AS shift_log ON shift_log.group_id = group_data.id "
        + "AND shift_log.user_id = ? WHERE LOWER(group_data.name) = LOWER(?) "
        + "OR LOWER(group_data.name) LIKE CONCAT('%', LOWER(?), '%') "
        + "OR LOWER(?) LIKE CONCAT('%', LOWER(group_data.name), '%') "
        + "GROUP BY group_data.id, group_data.name, group_data.badge, group_data.group_type "
        + "ORDER BY CASE WHEN LOWER(group_data.name) = LOWER(?) THEN 0 ELSE 1 END, "
        + "group_data.id LIMIT 1";
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(query)) {
      statement.setInt(1, Emulator.getIntUnixTimestamp() - (7 * 24 * 60 * 60));
      statement.setInt(2, userId);
      statement.setString(3, jobName);
      statement.setString(4, jobName);
      statement.setString(5, jobDisplayName);
      statement.setString(6, jobName);
      try (ResultSet set = statement.executeQuery()) {
        if (set.next()) {
          return new ProfileJobGroup(set.getInt("id"), set.getString("name"),
              set.getString("badge"), set.getInt("group_type"),
              set.getInt("weekly_shifts"), set.getInt("total_shifts"));
        }
      }
    } catch (SQLException e) {
      log.error("Unable to load profile job group for user {}", userId, e);
    }
    return null;
  }

  private static final class ProfileSkill {
    private final int id;
    private final int level;
    private final String iconLink;

    private ProfileSkill(int id, int level, String iconLink) {
      this.id = id;
      this.level = level;
      this.iconLink = iconLink;
    }
  }

  private static final class ProfileJobGroup {
    private final int id;
    private final String name;
    private final String badge;
    private final int kind;
    private final int weeklyShifts;
    private final int totalShifts;

    private ProfileJobGroup(int id, String name, String badge, int kind, int weeklyShifts,
        int totalShifts) {
      this.id = id;
      this.name = name;
      this.badge = badge;
      this.kind = kind;
      this.weeklyShifts = weeklyShifts;
      this.totalShifts = totalShifts;
    }
  }

  public HabboInfo getHabboInfo() {
    return habboInfo;
  }

  public Habbo getHabbo() {
    return habbo;
  }

  public GameClient getViewer() {
    return viewer;
  }
}
