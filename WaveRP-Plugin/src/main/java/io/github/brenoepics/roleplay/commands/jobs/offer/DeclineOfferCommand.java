package io.github.brenoepics.roleplay.commands.jobs.offer;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;

public class DeclineOfferCommand extends Command {
    public DeclineOfferCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length != 2) {
            gameClient.getHabbo().whisper(":declineoffer <code>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOfferManager().declineOffer(gameClient.getHabbo().getHabboInfo().getId(), params[1])) {
            gameClient.getHabbo().whisper("Offer declined!", RoomChatMessageBubbles.ALERT);
        }
        return true;
    }
}
