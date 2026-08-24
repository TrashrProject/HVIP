package io.github.brenoepics.roleplay.runnables;

import static io.github.brenoepics.roleplay.features.escort.EscortManager.ESCORT_VARIABLE;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;

/**
 * @author BrenoEpic
 */
public class HabboFHabbo implements Runnable {
  private final int directionOffset;
  private final Habbo follower;
  private final Habbo followed;
  private final Room room;

  private final int enableId;
  private final boolean fastWalkEnabled;

  /**
   * HabboFollowHabbo constructor
   *
   * @param follower Habbo of the client who will follow the other user
   * @param followed Habbo of the client who will be followed
   * @param room Room in which the follower will be following the followed
   * @param offset Offset of the direction in which the follower will be following the followed
   * @param enableId Followed user enable, e.g. police car
   */
  public HabboFHabbo(
      Habbo follower, Habbo followed, Room room, int offset, int enableId, boolean fastWalk) {
    this.follower = follower;
    this.followed = followed;
    this.room = room;
    this.directionOffset = offset;
    this.enableId = enableId;
    this.fastWalkEnabled = fastWalk;
  }

  @Override
  public void run() {
    if (!canRun(this.follower, this.followed)) return;

    // TODO: teleport to room

    RoomUnit followedUnit = this.followed.getRoomUnit();
    RoomUnit followerUnit = this.follower.getRoomUnit();

    if (fastWalkEnabled) {
      followedUnit.setFastWalk(true);
      followerUnit.setFastWalk(true);
    }

    if (enableId != 0) {
      giveEnable(followed);
      giveEnable(follower);
   }

    int direction = getDirectionOffset(followedUnit.getBodyRotation(), this.directionOffset);
    RoomTile target = getTileInFront(this.room, followedUnit.getCurrentLocation(), direction);

    if (target == null || !target.isWalkable()) {
      target = followedUnit.getCurrentLocation();
    }

    followerUnit.setGoalLocation(target);
    followerUnit.setCanWalk(true);

    Emulator.getThreading().run(this, 200);
  }

  private void giveEnable(Habbo habbo) {
    if (habbo.getRoomUnit().getEffectId() == enableId) return;

    room.giveEffect(habbo, enableId, -1);
  }

  private boolean canRun(Habbo follower, Habbo followed) {
    if (follower == null || followed == null) return false;

    int escortingUser = (int) this.follower.getHabboStats().cache.getOrDefault(ESCORT_VARIABLE, 0);
    if (escortingUser != this.followed.getHabboInfo().getId()) {
      return false;
    }

    // is at any room?
    return followed.getRoomUnit() != null && this.follower.getRoomUnit() != null;
  }

  private RoomTile getTileInFront(Room room, RoomTile tile, int rotation) {
    return room.getLayout().getTileInFront(tile, rotation);
  }

  private int getDirectionOffset(RoomUserRotation userRotation, int directionOffset) {
    return Math.abs((userRotation.getValue() + directionOffset + 4) % 8);
  }
}
