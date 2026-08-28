package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.items.interactions.interfaces.ConditionalGate;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUnitType;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobRankEntity;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import org.json.JSONObject;

public class InteractionJobGate extends InteractionDefault implements ConditionalGate {

  private final List<Integer> allowedJobs = new ArrayList<>();

  public InteractionJobGate(ResultSet set, Item baseItem) throws SQLException {
    super(set, baseItem);
    loadAllowedJobRanks();
  }

  public InteractionJobGate(int id, int userId, Item item, String extradata, int limitedStack,
      int limitedSells) {
    super(id, userId, item, extradata, limitedStack, limitedSells);
    loadAllowedJobRanks();
  }

  private void loadAllowedJobRanks() {
    String params = this.getBaseItem().getCustomParams();
    if (params == null || params.isEmpty()) {
      return;
    }
    JSONObject obj = new JSONObject(params);
    int jobId = obj.getInt("jobId");
    allowedJobs.add(jobId);
  }

  @Override
  public void setExtradata(String extradata) {
    super.setExtradata("0");
  }

  @Override
  public boolean canWalkOn(RoomUnit roomUnit, Room room, Object[] objects) {
    if (roomUnit == null || !roomUnit.getRoomUnitType().equals(RoomUnitType.USER)) {
      return false;
    }

    Habbo habbo = room.getHabbo(roomUnit);

    if (habbo == null) {
      return false;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);

    JobRankEntity job = avatar.getJobRankEntity();

    if (job == null || !avatar.isDuty()) {
      return false;
    }

    return isAllowed(job);
  }

  @Override
  public boolean isWalkable() {
    return true;
  }

  public boolean isAllowed(JobRankEntity jobRank) {
    if (jobRank == null) {
      return false;
    }
    return allowedJobs.stream().mapToInt(job -> job).anyMatch(job -> job == jobRank.getJobId());
  }

  @Override
  public void onRejected(RoomUnit roomUnit, Room room, Object[] objects) {

  }

  private record JobRankPair(int jobId, int rankId) {

  }
}
