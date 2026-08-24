package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class RoomHealCommand extends Command {

  public RoomHealCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) throws Exception {

    Habbo healer = gameClient.getHabbo();
    Room room = healer.getHabboInfo().getCurrentRoom();

    RoomChatMessage roomChatMessage = new RoomChatMessage(
        "Begins healing everyone in the room" + "*",
        healer.getRoomUnit(), RoomChatMessageBubbles.NORMAL);

    room.sendComposer(new RoomUserShoutComposer(roomChatMessage).compose());

    for (Habbo habbo : room.getHabbos()) {
      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (avatar != null) {
        avatar.heal();
        avatar.resetHungry();

      }
    }

    return true;
  }
}
