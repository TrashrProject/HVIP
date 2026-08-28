package com.eu.habbo.messages.outgoing.rooms;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.eu.habbo.messages.outgoing.Outgoing;

public class RoomModelComposer extends MessageComposer {

  private final Room room;

  public RoomModelComposer(Room room) {
    this.room = room;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(Outgoing.RoomModelComposer);
    this.response.appendString(this.room.getLayout().getName());
    this.response.appendInt(this.room.getId());
    serializeMapData();
    this.response.appendBoolean(true);
    this.response.appendInt(this.room.getWallHeight());
    this.response.appendString(this.room.getLayout().getRelativeMap());
    return this.response;
  }

  private void serializeMapData() {
    this.response.appendInt(
        this.room.getLayout().getMapSize() / this.room.getLayout().getMapSizeY());
    this.response.appendInt(this.room.getLayout().getMapSize());
    for (short y = 0; y < this.room.getLayout().getMapSizeY(); y++) {
      for (short x = 0; x < this.room.getLayout().getMapSizeX(); x++) {
        RoomTile t = this.room.getLayout().getTile(x, y);

        if (t != null) {
          if (Emulator.getConfig().getBoolean("custom.stacking.enabled")) {
            this.response.appendShort((short) (t.z * 256.0));
          } else {
            this.response.appendShort(t.relativeHeight());
          }
        } else {
          this.response.appendShort(Short.MAX_VALUE);
        }

      }
    }
  }

  public Room getRoom() {
    return room;
  }
}
