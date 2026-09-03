package com.eu.habbo.messages.outgoing.floorplaneditor;

import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.eu.habbo.messages.outgoing.Outgoing;
import gnu.trove.set.hash.THashSet;

public class FloorPlanEditorBlockedTilesComposer extends MessageComposer {
    private final Room room;
    private final boolean ignoreLockedTiles;

    public FloorPlanEditorBlockedTilesComposer(Room room) {
        this(room, false);
    }

    public FloorPlanEditorBlockedTilesComposer(Room room, boolean ignoreLockedTiles) {
        this.room = room;
        this.ignoreLockedTiles = ignoreLockedTiles;
    }

    @Override
    protected ServerMessage composeInternal() {
        this.response.init(Outgoing.FloorPlanEditorBlockedTilesComposer);

        THashSet<RoomTile> tileList = this.ignoreLockedTiles ? new THashSet<>() : this.room.getLockedTiles();

        this.response.appendInt(tileList.size());
        for (RoomTile node : tileList) {
            this.response.appendInt((int) node.x);
            this.response.appendInt((int) node.y);
        }

        return this.response;
    }

    public Room getRoom() {
        return room;
    }
}
