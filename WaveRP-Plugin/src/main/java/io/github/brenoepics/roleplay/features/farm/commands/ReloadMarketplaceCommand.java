package io.github.brenoepics.roleplay.features.farm.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;

public class ReloadMarketplaceCommand extends Command {

    public ReloadMarketplaceCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RolePlay.getMarketplaceManager().load();
        gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_update_item_marketplace.success"));
        return true;
    }

    private Integer parseAmount(String amountString) {
        try {
            int amount = Integer.parseInt(amountString);
            if (amount <= 0) throw new NumberFormatException("commands.cmd_sell_item.error.params.amount");

            return amount;
        } catch (NumberFormatException e) {
            throw new NumberFormatException("commands.cmd_sell_item.error.params.amount");
        }
    }
}
