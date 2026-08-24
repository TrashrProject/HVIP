package io.github.brenoepics.roleplay.features.user;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.google.gson.JsonElement;
import com.google.gson.JsonSyntaxException;
import io.github.brenoepics.roleplay.utilities.JsonFactory;
import java.util.Optional;
import lombok.AllArgsConstructor;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
@AllArgsConstructor
public class AvatarLocation {

  private final short x;
  private final short y;
  private final int rotation;
  private final int roomId;

  public JsonElement toJson() {
    return JsonFactory.getInstance().toJsonTree(this);
  }

  public static Optional<AvatarLocation> fromString(String jsonObject) {
    if (jsonObject == null || jsonObject.isEmpty()) {
      return Optional.empty();
    }

    try {
      return Optional.ofNullable(
          JsonFactory.getInstance().fromJson(jsonObject, AvatarLocation.class));
    } catch (JsonSyntaxException e) {
      return Optional.empty();
    }
  }

  public boolean teleportHabbo(Habbo habbo) {
    if (this.roomId <= 0) {
      return false;
    }

    Room room = Emulator.getGameEnvironment().getRoomManager().loadRoom(this.roomId);

    if (room == null) {
      return false;
    }

    if (room.isPreLoaded()) {
      room.loadData();
    }

    RoomTile targetTile = room.getLayout().getTile(this.x, this.y);

    if (targetTile == null) {
      return false;
    }

    Emulator.getGameEnvironment().getRoomManager().enterRoom(habbo, room.getId(), "",
        Emulator.getConfig().getBoolean("hotel.teleport.locked.allowed"), targetTile,
        RoomUserRotation.fromValue(rotation));
    return true;
  }

  private void walkOnTopItem(Habbo habbo, Room room) {
    HabboItem item = room.getTopItemAt(this.x, this.y);

    if (item != null) {
      try {
        item.onWalkOn(habbo.getRoomUnit(), room, null);
      } catch (Exception e) {
        //ignored
      }
    }
  }

  private static RoomUnit getRoomUnit(Habbo habbo) {
    return habbo.getRoomUnit();
  }
}