package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionTeleport;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.runnables.TeleportActionOne;
import java.sql.ResultSet;
import java.sql.SQLException;

public class InteractionRPTeleportTile extends InteractionTeleport {

  public InteractionRPTeleportTile(ResultSet set, Item baseItem) throws SQLException {
    super(set, baseItem);
  }

  public InteractionRPTeleportTile(int id, int userId, Item item, String extradata,
      int limitedStack, int limitedSells) {
    super(id, userId, item, extradata, limitedStack, limitedSells);
  }

  @Override
  public void setExtradata(String extradata) {
    super.setExtradata("0");
  }

  @Override
  public boolean canWalkOn(RoomUnit roomUnit, Room room, Object[] objects) {
    return true;
  }

  @Override
  public boolean isWalkable() {
    return true;
  }

  @Override
  public void onClick(GameClient client, Room room, Object[] objects) {
    // Not needed
  }

  @Override
  public void onWalkOn(RoomUnit roomUnit, Room room, Object[] objects) {
    if (roomUnit == null || !this.canWalkOn(roomUnit, room, objects) || !this.getOccupyingTiles(
        room.getLayout()).contains(roomUnit.getGoal())) {
      return;
    }

    Habbo habbo = room.getHabbo(roomUnit);
    if (room.getHabbo(roomUnit) == null || habbo.getClient() == null || !canUseTeleport(
        habbo.getClient(), room) || habbo.getRoomUnit().isTeleporting
        || roomUnit.tilesWalked() == 0) {
      return;
    }

    this.startTeleport(room, habbo, 0);
  }

  @Override
  public void startTeleport(Room room, Habbo habbo, int delay) {
    if (this.getOccupyingTiles(room.getLayout()).contains(habbo.getRoomUnit().getGoal())) {
      Emulator.getThreading().run(new TeleportActionOne(this, room, habbo.getClient()), delay);
    }
  }
}
