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

        if (params.length < 3) {
            gameClient.getHabbo().whisper("Syntaxe : :vendre <pseudo> <objet>", RoomChatMessageBubbles.ALERT);
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
            gameClient.getHabbo().whisper("Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
            return true;
        }

        String itemName = String.join(" ", java.util.Arrays.copyOfRange(params, 2, params.length));
        RPItem item = data.getInventory().getSlotItem(itemName);

        if (item == null) {
            gameClient.getHabbo().whisper("Vous ne poss\u00e9dez pas cet objet.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOfferManager().isBeingOffered(item.getId())) {
            gameClient.getHabbo().whisper("Cet objet est d\u00e9j\u00e0 propos\u00e9. Utilisez :effaceroffres.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getOfferManager().handleSellItem(item, data, gameClient.getHabbo(), habbo);
        RolePlay.getCommandsCounter().getCoolDown("offer").addTimeOut(habbo.getHabboInfo().getId(), OFFER_TIMEOUT);
        return true;
    }
}
