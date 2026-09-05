package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;

public final class RoleplayChat {

  private RoleplayChat() {
  }

  public static void shoutAction(Habbo habbo, String message) {
    if (habbo == null || !habbo.getRoomUnit().isInRoom()
        || habbo.getHabboInfo().getCurrentRoom() == null) {
      return;
    }

    // Keep the RP action markers inside the actual chat text so Nitro renders:
    // Pseudo: * Mange ... *
    // instead of action mode moving the first asterisk before the username.
    habbo.getHabboInfo().getCurrentRoom().sendComposer(
        new RoomUserShoutComposer(
            new RoomChatMessage(message, habbo.getRoomUnit(), RoomChatMessageBubbles.NORMAL)).compose());
  }
}
