package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;

public class BuildModeCommand extends Command {
    public BuildModeCommand() {
        super("acc_placefurni", new String[]{"buildmode"});
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        Room room = gameClient.getHabbo().getHabboInfo().getCurrentRoom();
        if (room == null || !room.hasRights(gameClient.getHabbo())) {
            return false;
        }

        boolean enabled = ParadiseBuildState.toggleBuildMode(gameClient.getHabbo());
        if (enabled) {
            gameClient.getHabbo().whisper("BuildMode activé : les mobis déplacés ou posés sont forcés en Z = 0.", RoomChatMessageBubbles.ALERT);
        } else {
            gameClient.getHabbo().whisper("BuildMode désactivé.", RoomChatMessageBubbles.ALERT);
        }
        return true;
    }

    @Override
    public boolean handlePermissionDenied(GameClient gameClient, String[] params) throws Exception {
        Room room = gameClient.getHabbo().getHabboInfo().getCurrentRoom();
        if (room != null && room.hasRights(gameClient.getHabbo())) {
            return handle(gameClient, params);
        }
        return false;
    }
}
