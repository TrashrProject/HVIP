package com.eu.habbo.messages.incoming.floorplaneditor;

import com.eu.habbo.habbohotel.commands.NoItemFloorCommand;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.messages.outgoing.floorplaneditor.FloorPlanEditorBlockedTilesComposer;

public class FloorPlanEditorRequestBlockedTilesEvent extends MessageHandler {
    @Override
    public void handle() throws Exception {
        Room room = this.client.getHabbo().getHabboInfo().getCurrentRoom();
        if (room == null)
            return;

        boolean noItemFloor = NoItemFloorCommand.isEnabled(this.client.getHabbo(), room);
        this.client.sendResponse(new FloorPlanEditorBlockedTilesComposer(room, noItemFloor));
    }
}
