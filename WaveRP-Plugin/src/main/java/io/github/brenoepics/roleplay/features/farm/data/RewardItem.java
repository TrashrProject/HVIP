package io.github.brenoepics.roleplay.features.farm.data;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.achievements.Achievement;
import com.eu.habbo.habbohotel.achievements.AchievementManager;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.ResultSet;
import java.sql.SQLException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class RewardItem {

  private static final Logger log = LoggerFactory.getLogger(RewardItem.class);
  private final int id;
  private final int rp_item;
  private final int credits;
  private final int points;
  private final int points_type;
  private final int duckets;
  private final String badge;

  public RewardItem(ResultSet result) throws SQLException {
    this.id = result.getInt("id");
    this.rp_item = result.getInt("rp_item");
    this.credits = result.getInt("credits");
    this.points = result.getInt("points");
    this.points_type = result.getInt("points_type");
    this.duckets = result.getInt("duckets");
    this.badge = result.getString("badge");
  }

  public int getId() {
    return this.id;
  }

  public void give(Habbo habbo) {
    if (this.credits > 0) {
      habbo.giveCredits(this.credits);
    }
    if (this.points > 0) {
      habbo.givePoints(this.points_type, this.points);
    }
    if (this.duckets > 0) {
      habbo.givePixels(this.duckets);
    }

    if (!this.badge.isEmpty()) {
      habbo.addBadge(this.badge);
    }

    if (this.rp_item < 1) {
      return;
    }

    RPItem rewardItem = RolePlay.getItemManager().getItemById(this.rp_item);
    if (rewardItem == null) {
      log.error("Could not find rp_item with id {}", this.rp_item);
      return;
    }

    Achievement achievement = Emulator.getGameEnvironment().getAchievementManager()
        .getAchievement("farm_" + this.id + "_reward");
    if (achievement != null) {
      AchievementManager.progressAchievement(habbo, achievement, 1);
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    data.getInventory().addItem(habbo, rewardItem, 1);
  }

}
