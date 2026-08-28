package io.github.brenoepics.roleplay.communication.incoming.roleplay;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.outgoing.roleplay.OpenInventoryComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class OpenInventoryEvent extends IncomingWebMessage<OpenInventoryEvent.JSONOpenInventoryEvent> {

    public OpenInventoryEvent() {
        super(JSONOpenInventoryEvent.class);
    }

    @Override
    public void handle(GameClient client, JSONOpenInventoryEvent message) {
        Habbo habbo = client.getHabbo();
        if (habbo == null || habbo.getRoomUnit() == null)
            return;

        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
        OpenInventoryComposer openInventoryComposer = new OpenInventoryComposer(data.getInventory(), Emulator.getConfig().getValue("features.inventory.image.url"), true);
        habbo.getClient().sendResponse(new JavascriptCallbackComposer(openInventoryComposer));
    }

    static class JSONOpenInventoryEvent {
    }
}
