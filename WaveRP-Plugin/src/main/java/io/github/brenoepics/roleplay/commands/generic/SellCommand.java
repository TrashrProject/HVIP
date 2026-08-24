package io.github.brenoepics.roleplay.commands.generic;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.OFFER_TIMEOUT;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class SellCommand extends Command {
    public SellCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (params.length != 3) {
            gameClient.getHabbo().whisper(":offer <name> <item>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            gameClient.getHabbo().whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (habbo == gameClient.getHabbo()) {
            gameClient.getHabbo().whisper("You cannot offer anything to yourself", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("offer").getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
        if (timeout != null) {
            gameClient.getHabbo().whisper("You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " seconds to use this command again!");
            return true;
        }

        RPItem item = data.getInventory().getSlotItem(params[2]);

        if (item == null) {
            gameClient.getHabbo().whisper("You don't have this item", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOfferManager().isBeingOffered(item.getId())) {
            gameClient.getHabbo().whisper("You are already offering this item, use :clearoffers", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getOfferManager().handleSellItem(item, data, gameClient.getHabbo(), habbo);
        RolePlay.getCommandsCounter().getCoolDown("offer").addTimeOut(habbo.getHabboInfo().getId(), OFFER_TIMEOUT);
        return true;
    }
}
