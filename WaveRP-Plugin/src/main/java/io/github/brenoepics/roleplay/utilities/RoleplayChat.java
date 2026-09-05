package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;

public final class RoleplayChat {

  private RoleplayChat() {
  }

  public static void shoutAction(Habbo habbo, String message) {
    if (habbo == null || habbo.getRoomUnit() == null
        || habbo.getHabboInfo().getCurrentRoom() == null) {
      return;
    }

    // Utiliser la meme construction de RoomChatMessage que les interactions RP
    // deja fonctionnelles (ex. poubelles). Cela garantit l'affichage de la bulle
    // au-dessus du Habbo et dans le chat de la salle sur Nitro.
    habbo.getHabboInfo().getCurrentRoom().sendComposer(
        new RoomUserShoutComposer(
            new RoomChatMessage(
                message,
                habbo,
                habbo,
                RoomChatMessageBubbles.NORMAL)).compose());
  }
}
