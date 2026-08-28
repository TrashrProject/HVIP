package io.github.brenoepics.roleplay.communication.packets.emulator.incoming;

import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.RolePlay;

/**
 * Requests the latest banking snapshot (ATM + Phone composers) for the requesting user.
 * Custom packet id registered in EmulatorLoad (3325).
 * No payload required.
 */
public class RequestBankDataEvent extends MessageHandler {
    @Override
    public void handle() throws Exception {
        if (this.client == null || this.client.getHabbo() == null) return;
        int userId = this.client.getHabbo().getHabboInfo().getId();
        RolePlay.getBankManager().sendBankSnapshot(userId);
    }
}

