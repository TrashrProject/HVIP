package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.rooms.RoomLoadedEvent;

public class RoomLoadedListener implements EventListener {

    @EventHandler
    public static void onRoomLoad(RoomLoadedEvent event) {
        if (event.room.getCategory() == Emulator.getConfig().getInt("nahabbo.features.room.category")) {
            event.room.setAllowWalkthrough(true);
        }
    }
}
