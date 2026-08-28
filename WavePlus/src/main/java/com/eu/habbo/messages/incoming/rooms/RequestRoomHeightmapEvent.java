package com.eu.habbo.messages.incoming.rooms;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.messages.outgoing.rooms.RoomHeightMapComposer;
import com.eu.habbo.messages.outgoing.rooms.RoomRelativeMapComposer;

public class RequestRoomHeightmapEvent extends MessageHandler {

  public static void enterRoom(GameClient client, Room room) {
    if (room == null || room.getLayout() == null) {
      return;
    }

    client.sendResponse(new RoomRelativeMapComposer(room));
    client.sendResponse(new RoomHeightMapComposer(room));

    Emulator.getGameEnvironment().getRoomManager().enterRoom(client.getHabbo(), room);
  }

  @Override
  public void handle() throws Exception {
    if (this.client.getHabbo().getHabboInfo().getLoadingRoom() <= 0) {
      return;
    }

    Room room = Emulator.getGameEnvironment().getRoomManager()
        .loadRoom(this.client.getHabbo().getHabboInfo().getLoadingRoom());
    enterRoom(this.client, room);
  }
}
