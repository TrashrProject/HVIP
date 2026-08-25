package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomManager;
import java.util.Comparator;
import java.util.List;

/**
 * :hotrooms - Lists the top 5 rooms with the most people currently inside.
 */
public class HotRoomsCommand extends Command {

  public HotRoomsCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    // Only accepts :hotrooms (no args)
    if (params.length != 1) {
      gameClient.getHabbo().whisper(":salles", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RoomManager rm = Emulator.getGameEnvironment().getRoomManager();
    List<Room> rooms = rm.getActiveRooms();

    if (rooms.isEmpty()) {
      gameClient.getHabbo().whisper("Aucune salle active n'a \u00e9t\u00e9 trouv\u00e9e.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    // Sort by current number of habbos in the room, desc, take top 5
    List<Room> top = rooms.stream()
        .sorted(Comparator.comparingInt((Room r) -> r.getHabbos().size()).reversed())
        .limit(5)
        .toList();

    if (top.isEmpty()) {
      gameClient.getHabbo()
          .whisper("Aucune salle n'est actuellement occup\u00e9e.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    StringBuilder sb = new StringBuilder();
    sb.append("Salles les plus fr\u00e9quent\u00e9es :\n");
    int i = 1;
    for (Room r : top) {
      int count = r.getHabbos().size();
      if (count <= 0) {
        continue; // show only rooms with people in them
      }
      sb.append(i++).append(". ").append(r.getName()).append(" (ID ").append(r.getId())
          .append(") - ")
          .append(count).append(count == 1 ? " joueur" : " joueurs").append("\n");
    }

    if (sb.toString().trim().equals("Salles les plus fr\u00e9quent\u00e9es :")) {
      gameClient.getHabbo()
          .whisper("Aucune salle n'est actuellement occup\u00e9e.", RoomChatMessageBubbles.ALERT);
    } else {
      gameClient.getHabbo().whisper(sb.toString().trim(), RoomChatMessageBubbles.ALERT);
    }
    return true;
  }
}
