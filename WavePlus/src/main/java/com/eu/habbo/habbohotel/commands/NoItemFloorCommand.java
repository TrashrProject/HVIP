package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.floorplaneditor.FloorPlanEditorBlockedTilesComposer;
import com.eu.habbo.messages.outgoing.floorplaneditor.FloorPlanEditorDoorSettingsComposer;
import com.eu.habbo.messages.outgoing.rooms.RoomFloorThicknessUpdatedComposer;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class NoItemFloorCommand extends Command {
    private static final Map<Integer, Integer> ENABLED_ROOMS = new ConcurrentHashMap<>();

    public NoItemFloorCommand() {
        super(null, new String[]{"noitemfloor"});
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        Room room = ParadiseFloorLayoutSupport.getCurrentRoom(gameClient);
        if (room == null) {
            return true;
        }

        if (!ParadiseFloorLayoutSupport.canEditLayout(gameClient, room)) {
            gameClient.getHabbo().whisper("Tu dois être propriétaire de l'appart pour utiliser :noitemfloor.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        enable(gameClient.getHabbo(), room);

        // Acknowledge immediately. The Nitro client bridge then opens its native floor editor
        // by executing Nitro's existing local :floor command after this server command is sent.
        gameClient.getHabbo().whisper(
                "NoItemFloor activé : ouverture de l'éditeur de sol. Les cases occupées par des mobis restent modifiables jusqu'à la sauvegarde.",
                RoomChatMessageBubbles.ALERT
        );

        // Feed the floor editor immediately with an unlocked tile list. If the editor asks
        // again after opening, FloorPlanEditorRequestBlockedTilesEvent applies the same mode.
        gameClient.sendResponse(new FloorPlanEditorBlockedTilesComposer(room, true));
        gameClient.sendResponse(new FloorPlanEditorDoorSettingsComposer(room));
        gameClient.sendResponse(new RoomFloorThicknessUpdatedComposer(room));

        return true;
    }

    public static void enable(Habbo habbo, Room room) {
        if (habbo != null && room != null) {
            ENABLED_ROOMS.put(habbo.getHabboInfo().getId(), room.getId());
        }
    }

    public static boolean isEnabled(Habbo habbo, Room room) {
        if (habbo == null || room == null) {
            return false;
        }

        Integer roomId = ENABLED_ROOMS.get(habbo.getHabboInfo().getId());
        return roomId != null && roomId == room.getId();
    }

    public static void disable(Habbo habbo) {
        if (habbo != null) {
            ENABLED_ROOMS.remove(habbo.getHabboInfo().getId());
        }
    }
}
