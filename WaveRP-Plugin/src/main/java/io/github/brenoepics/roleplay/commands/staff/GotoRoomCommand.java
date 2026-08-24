package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;

public class GotoRoomCommand extends Command {

  public GotoRoomCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) {
    Habbo staff = gameClient.getHabbo();

    if (strings.length < 2) {
      staff.whisper(":goto <roomid>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int roomId;
    try {
      roomId = Integer.parseInt(strings[1]);
    } catch (NumberFormatException e) {
      staff.whisper("Invalid room ID.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Room room = Emulator.getGameEnvironment().getRoomManager().loadRoom(roomId);
    if (room == null) {
      staff.whisper("Room with ID " + roomId + " not found.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    staff.goToRoom(roomId);
    staff.whisper("You have been sent to room " + roomId + ".", RoomChatMessageBubbles.NORMAL);

    return true;
  }
}