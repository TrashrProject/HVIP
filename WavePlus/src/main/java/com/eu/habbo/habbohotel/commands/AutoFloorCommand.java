package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomLayout;
import com.eu.habbo.habbohotel.rooms.RoomTile;

public class AutoFloorCommand extends Command {
    public AutoFloorCommand() {
        super(null, new String[]{"autofloor"});
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        Room room = ParadiseFloorLayoutSupport.getCurrentRoom(gameClient);
        if (room == null) {
            return true;
        }

        if (!ParadiseFloorLayoutSupport.canEditLayout(gameClient, room)) {
            gameClient.getHabbo().whisper("Tu dois être propriétaire de l'appart pour utiliser :autofloor.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RoomLayout layout = room.getLayout();
        if (layout == null || layout.getHeightmap() == null || layout.getHeightmap().isEmpty()) {
            gameClient.getHabbo().whisper("Impossible de lire le floor actuel.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        String[] rawRows = layout.getHeightmap().replace("X", "x").split("\\r");
        StringBuilder map = new StringBuilder();
        int keptTiles = 0;

        for (int y = 0; y < rawRows.length; y++) {
            String row = rawRows[y].replaceAll("[^0-9a-zA-Z]", "");
            if (row.isEmpty()) {
                continue;
            }

            for (int x = 0; x < row.length(); x++) {
                boolean isDoor = layout.getDoorX() == x && layout.getDoorY() == y;
                RoomTile tile = layout.getTile((short) x, (short) y);
                boolean hasFurniture = tile != null && !room.getItemsAt(tile).isEmpty();

                if (isDoor || hasFurniture) {
                    map.append(row.charAt(x));
                    keptTiles++;
                } else {
                    map.append('x');
                }
            }

            map.append('\r');
        }

        boolean applied = ParadiseFloorLayoutSupport.applyLayout(
                gameClient,
                room,
                map.toString(),
                layout.getDoorX(),
                layout.getDoorY(),
                layout.getDoorDirection(),
                room.getWallSize(),
                room.getFloorSize(),
                room.getWallHeight()
        );

        if (applied) {
            gameClient.getHabbo().whisper("AutoFloor terminé : seules les cases utiles autour des mobis ont été conservées (" + keptTiles + " cases).", RoomChatMessageBubbles.ALERT);
        } else {
            gameClient.getHabbo().whisper("AutoFloor n'a pas pu appliquer le nouveau floor.", RoomChatMessageBubbles.ALERT);
        }

        return true;
    }
}
