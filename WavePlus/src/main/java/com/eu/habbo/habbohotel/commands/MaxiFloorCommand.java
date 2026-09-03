package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;

public class MaxiFloorCommand extends Command {
    public MaxiFloorCommand() {
        super(null, new String[]{"maxifloor", "maxfloor"});
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        Room room = ParadiseFloorLayoutSupport.getCurrentRoom(gameClient);
        if (room == null) {
            return true;
        }

        if (!ParadiseFloorLayoutSupport.canEditLayout(gameClient, room)) {
            gameClient.getHabbo().whisper("Tu dois être propriétaire de l'appart pour utiliser :maxifloor.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        // MaxFloor changes the whole room model. Refuse to do it with furniture present so
        // furniture coordinates can never be orphaned or silently destroyed.
        if (!room.getFloorItems().isEmpty() || !room.getWallItems().isEmpty()) {
            gameClient.getHabbo().whisper("Retire d'abord les mobis de l'appart avant d'utiliser :maxifloor.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        int size = ParadiseFloorLayoutSupport.MAXI_FLOOR_SIZE;
        StringBuilder map = new StringBuilder(size * (size + 1));

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                if (y == 0) {
                    map.append('x');
                } else if (y == 1 && x == 0) {
                    map.append('0');
                } else if (x == 0) {
                    map.append('x');
                } else {
                    map.append('0');
                }
            }
            map.append('\r');
        }

        // applyLayout() reloads the room. Send the confirmation first so Nitro always shows
        // visible feedback instead of losing the whisper while the client is being forwarded.
        gameClient.getHabbo().whisper(
                "MaxiFloor : passage de l'appart en " + size + "x" + size + ". Rechargement en cours...",
                RoomChatMessageBubbles.ALERT
        );

        boolean applied = ParadiseFloorLayoutSupport.applyLayout(
                gameClient,
                room,
                map.toString(),
                0,
                1,
                2,
                0,
                0,
                -1
        );

        if (!applied) {
            gameClient.getHabbo().whisper("MaxiFloor n'a pas pu appliquer le nouveau floor.", RoomChatMessageBubbles.ALERT);
        }

        return true;
    }
}
