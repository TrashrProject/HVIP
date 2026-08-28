package io.github.brenoepics.roleplay.communication.incoming;

import static io.github.brenoepics.roleplay.features.escort.EscortManager.ESCORT_VARIABLE;

import com.eu.habbo.Emulator;
import com.eu.habbo.messages.incoming.rooms.users.RoomUserWalkEvent;

public class RoomUserWalkEventPlugin extends RoomUserWalkEvent {
    @Override
    public int getRatelimit() {
        return 0;
    }

    @Override
    public void handle() throws Exception {
        Object escorting = this.client.getHabbo().getHabboStats().cache.get(ESCORT_VARIABLE);
        if (escorting instanceof Number && ((Number) escorting).intValue() > 0) return;

        super.handle();
    }
}
