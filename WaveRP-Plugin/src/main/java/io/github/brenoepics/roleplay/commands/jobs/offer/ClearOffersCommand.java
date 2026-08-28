package io.github.brenoepics.roleplay.commands.jobs.offer;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;

public class ClearOffersCommand extends Command {
    public ClearOffersCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RolePlay.getOfferManager().clearOffers(gameClient.getHabbo());
        return true;
    }
}
