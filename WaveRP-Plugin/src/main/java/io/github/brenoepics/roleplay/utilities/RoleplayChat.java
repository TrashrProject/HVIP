package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;

public final class RoleplayChat {

  private RoleplayChat() {
  }

  public static void shoutAction(Habbo habbo, String message) {
    if (habbo == null || !habbo.getRoomUnit().isInRoom()
        || habbo.getHabboInfo().getCurrentRoom() == null) {
      return;
    }

    habbo.getHabboInfo().getCurrentRoom().sendComposer(
        new RoomUserShoutComposer(new ActionChatMessage(message, habbo.getRoomUnit())).compose());
  }

  private static final class ActionChatMessage extends RoomChatMessage {

    private ActionChatMessage(String message, RoomUnit roomUnit) {
      super(message, roomUnit, RoomChatMessageBubbles.NORMAL);
    }

    @Override
    public void serialize(ServerMessage message) {
      super.serialize(message);
      message.appendInt(1);
    }
  }
}
