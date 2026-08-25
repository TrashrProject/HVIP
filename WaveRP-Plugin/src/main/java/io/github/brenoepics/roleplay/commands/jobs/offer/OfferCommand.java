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
            gameClient.getHabbo().whisper(":proposer <pseudo> <objet>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (data.getJobEntity() == null) {
            gameClient.getHabbo().whisper("Vous n'avez aucun m\u00e9tier.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.isDuty()) {
            gameClient.getHabbo().whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            gameClient.getHabbo().whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (habbo == gameClient.getHabbo()) {
            gameClient.getHabbo().whisper("Vous ne pouvez rien vous proposer \u00e0 vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("offer").getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
        if (timeout != null) {
            long secondsLeft = timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond();
            if (secondsLeft < 0) secondsLeft = 0;
            gameClient.getHabbo().whisper("Vous devez attendre " + secondsLeft + " seconde(s) avant de r\u00e9utiliser cette commande.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOfferManager().handleOfferItem(params[2], data, gameClient.getHabbo(), habbo))
            RolePlay.getCommandsCounter().getCoolDown("offer").addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), OFFER_TIMEOUT);

        return true;
    }
}
