package com.eu.habbo.plugin.events.users;

import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUserRotation;
import com.eu.habbo.habbohotel.users.Habbo;
import lombok.Getter;
import lombok.Setter;

public class UserEnterRoomEvent extends UserEvent {

  public final Room room;
  @Getter
  @Setter
  private RoomTile doorTile;
  @Getter
  @Setter
  private RoomUserRotation rotation;

  public UserEnterRoomEvent(Habbo habbo, Room room, RoomTile doorTile, RoomUserRotation rotation) {
    super(habbo);

    this.room = room;
    this.doorTile = doorTile;
    this.rotation = rotation;
  }
}
