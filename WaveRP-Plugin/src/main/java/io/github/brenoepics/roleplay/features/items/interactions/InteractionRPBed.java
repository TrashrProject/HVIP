package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUnitType;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import java.sql.ResultSet;
import java.sql.SQLException;

public class InteractionRPBed extends InteractionDefault {

  public InteractionRPBed(ResultSet set, Item baseItem) throws SQLException {
    super(set, baseItem);
  }

  public InteractionRPBed(int id, int userId, Item item, String extradata, int limitedStack,
      int limitedSells) {
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
  public void onWalkOff(RoomUnit roomUnit, Room room, Object[] objects) throws Exception {
    super.onWalkOff(roomUnit, room, objects);
    if (roomUnit == null || !roomUnit.getRoomUnitType().equals(RoomUnitType.USER)) {
      return;
    }

    Habbo habbo = room.getHabbo(roomUnit);
    if (habbo != null) {
      RolePlay.getHospitalService().removeGlow(habbo);
    }
  }
}
