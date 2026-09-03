package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.permissions.Permission;
import com.eu.habbo.habbohotel.rooms.CustomRoomLayout;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomLayout;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.rooms.ForwardToRoomComposer;

import java.util.ArrayList;
import java.util.Collection;

final class ParadiseFloorLayoutSupport {
    static final int MAXI_FLOOR_SIZE = 50;

    private ParadiseFloorLayoutSupport() {
    }

    static Room getCurrentRoom(GameClient gameClient) {
        if (gameClient == null || gameClient.getHabbo() == null) {
            return null;
        }

        return gameClient.getHabbo().getHabboInfo().getCurrentRoom();
    }

    static boolean canEditLayout(GameClient gameClient, Room room) {
        if (gameClient == null || gameClient.getHabbo() == null || room == null) {
            return false;
        }

        return room.getOwnerId() == gameClient.getHabbo().getHabboInfo().getId()
                || gameClient.getHabbo().hasPermission(Permission.ACC_ANYROOMOWNER);
    }

    static boolean applyLayout(GameClient gameClient,
                               Room room,
                               String map,
                               int doorX,
                               int doorY,
                               int doorRotation,
                               int wallSize,
                               int floorSize,
                               int wallHeight) throws Exception {
        if (room == null || map == null || map.isEmpty()) {
            return false;
        }

        RoomLayout layout = room.getLayout();

        if (layout instanceof CustomRoomLayout) {
            layout.setDoorX((short) doorX);
            layout.setDoorY((short) doorY);
            layout.setDoorDirection(doorRotation);
            layout.setHeightmap(map);
            layout.parse();

            if (layout.getDoorTile() == null) {
                gameClient.getHabbo().alert("Le floor généré est invalide : aucune porte valide n'a été trouvée.");
                ((CustomRoomLayout) layout).needsUpdate(false);
                return false;
            }

            // Persist synchronously before the room reload. Using the async runner here can
            // race with unload/load and bring back the previous heightmap.
            ((CustomRoomLayout) layout).needsUpdate(true);
            ((CustomRoomLayout) layout).run();
        } else {
            layout = Emulator.getGameEnvironment().getRoomManager()
                    .insertCustomLayout(room, map, doorX, doorY, doorRotation);
        }

        if (layout == null) {
            return false;
        }

        room.setHasCustomLayout(true);
        room.setNeedsUpdate(true);
        room.setLayout(layout);
        room.setWallSize(wallSize);
        room.setFloorSize(floorSize);
        room.setWallHeight(wallHeight);
        room.save();

        int roomId = room.getId();
        Collection<Habbo> habbos = new ArrayList<>(room.getUserCount());
        habbos.addAll(room.getHabbos());

        Emulator.getGameEnvironment().getRoomManager().unloadRoom(room);
        Room reloadedRoom = Emulator.getGameEnvironment().getRoomManager().loadRoom(roomId);
        if (reloadedRoom == null) {
            return false;
        }

        ServerMessage message = new ForwardToRoomComposer(roomId).compose();
        for (Habbo habbo : habbos) {
            if (habbo != null && habbo.getClient() != null) {
                habbo.getClient().sendResponse(message);
            }
        }

        return true;
    }
}
