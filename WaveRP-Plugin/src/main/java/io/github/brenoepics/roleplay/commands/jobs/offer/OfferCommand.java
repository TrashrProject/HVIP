package io.github.brenoepics.roleplay.commands.jobs.offer;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.OFFER_TIMEOUT;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class OfferCommand extends Command {
    public OfferCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (params.length != 3) {
            gameClient.getHabbo().whisper(":offer <name> <item>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (data.getJobEntity() == null) {
            gameClient.getHabbo().whisper("You are unemployed", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.isDuty()) {
            gameClient.getHabbo().whisper("You are not on duty", RoomChatMessageBubbles.ALERT);
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
            long secondsLeft = timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond();
            if (secondsLeft < 0) secondsLeft = 0;
            gameClient.getHabbo().whisper("You have to wait " + secondsLeft + " seconds to use this command again!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOfferManager().handleOfferItem(params[2], data, gameClient.getHabbo(), habbo))
            RolePlay.getCommandsCounter().getCoolDown("offer").addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), OFFER_TIMEOUT);

        return true;
    }
}
