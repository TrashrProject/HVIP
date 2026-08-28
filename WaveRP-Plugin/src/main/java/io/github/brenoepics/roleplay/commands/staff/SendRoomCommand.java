package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;

public class SendRoomCommand extends Command {

  public SendRoomCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) {
    Habbo staff = gameClient.getHabbo();

    if (strings.length < 3) {
      staff.whisper(":envoyersalle <pseudo> <id_salle>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String username = strings[1];
    String roomIdStr = strings[2];

    Habbo target = Emulator.getGameEnvironment().getHabboManager().getHabbo(username);
    if (target == null) {
      staff.whisper("Le joueur " + username + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int roomId;
    try {
      roomId = Integer.parseInt(roomIdStr);
    } catch (NumberFormatException e) {
      staff.whisper("L'ID de la salle est invalide.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Room room = Emulator.getGameEnvironment().getRoomManager().loadRoom(roomId);
    if (room == null) {
      staff.whisper("La salle " + roomId + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    target.goToRoom(roomId);
    staff.shout("* Envoie " + username + " dans la salle " + roomId + " *", RoomChatMessageBubbles.NORMAL);
    target.whisper("Un membre du staff vous a envoy\u00e9 dans la salle " + roomId + ".",
        RoomChatMessageBubbles.ALERT);

    return true;
  }
}
