package io.github.brenoepics.roleplay.features.farm.commands;

import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.farm.marketplace.SellableItem;
import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;

import java.util.Optional;

public class SellItemCommand extends Command {

    public SellItemCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 3) {
            gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_sell_item.error.params"));
            return false;
        }

        Optional<SellableItem> itemOptional = RolePlay.getMarketplaceManager().getItemByName(params[1]);
        if (!itemOptional.isPresent()) {
            gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_sell_item.error.params.item").replace("%item%", params[1]));
            return true;
        }

        SellableItem item = itemOptional.get();
        if (!item.getRooms().contains(gameClient.getHabbo().getHabboInfo().getCurrentRoom().getId())) {
            gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_sell_item.error.wrong_room"));
            return true;
        }

        boolean sellEverything = params[2].equalsIgnoreCase("all");

        Integer amount = null;
        if (!sellEverything) {
            try {
                amount = parseAmount(params[2]);
            } catch (NumberFormatException e) {
                gameClient.getHabbo().whisper(Emulator.getTexts().getValue(e.getMessage()));
                return false;
            }
        }

        try {
            RolePlay.getMarketplaceManager().sellItem(gameClient.getHabbo(), item, amount);
        } catch (Exception e) {
            gameClient.getHabbo().whisper(e.getMessage());
            return false;
        }

        gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_sell_item.successfully").replace("%amount%", amount == null ? "all" : amount.toString()).replace("%item%", item.getName()));
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
