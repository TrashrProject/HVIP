package io.github.brenoepics.roleplay.commands.jobs.offer;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;

public class AcceptOfferCommand extends Command {
    public AcceptOfferCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length != 2) {
            gameClient.getHabbo().whisper(":acceptoffer <code>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getOfferManager().acceptOffer(gameClient.getHabbo(), params[1]);
        return true;
    }
}

