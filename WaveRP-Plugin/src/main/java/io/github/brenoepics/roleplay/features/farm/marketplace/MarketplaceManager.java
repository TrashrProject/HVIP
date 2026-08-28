package io.github.brenoepics.roleplay.features.farm.marketplace;

import io.github.brenoepics.roleplay.features.farm.marketplace.exception.SellItemException;
import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.outgoing.inventory.RemoveHabboItemComposer;
import lombok.Getter;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.*;
import java.util.stream.Collectors;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class MarketplaceManager {

    private static final Logger log = LoggerFactory.getLogger(MarketplaceManager.class);
    @Getter
    private final Map<Integer, SellableItem> sellableItems = new HashMap<>();

    public MarketplaceManager() {
        this.load();
    }

    public void load() {
        this.sellableItems.clear();
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `items_sellable`");
             final ResultSet set = statement.executeQuery()) {
            while (set.next()) {
                final SellableItem item = new SellableItem(set.getInt("id"), set.getString("name"), set.getInt("item_id"), Arrays.asList(set.getString("rooms").split(",")), set.getInt("credits"), set.getInt("currency_type"), set.getInt("currency_amount"));
                this.sellableItems.put(item.getId(), item);
            }
        } catch (SQLException e) {
            log.error("[Marketplace-Manager]", e);
        } finally {
            log.info("[Marketplace-Manager] Loaded {} items successfully!", this.sellableItems.size());
        }
    }

    public void sellItem(Habbo seller, SellableItem item, Integer amount) throws SellItemException {
        Optional<Collection<HabboItem>> habboItems = this.getHabboItemsBySellable(seller, item);

        if (habboItems.isEmpty()) {
            throw new SellItemException(SellItemException.SIException.NOT_ENOUGH_ITEMS);
        }

        int sellAmount = amount == null ? habboItems.get().size() : amount;
        Collection<HabboItem> items = habboItems.get();
        if(items.size() < sellAmount) {
            throw new SellItemException(SellItemException.SIException.NOT_ENOUGH_ITEMS);
        }

        List<HabboItem> itemsToSell = items.stream().limit(sellAmount).toList();
        deleteHabboItems(seller, itemsToSell);
        rewardHabbo(seller, item, sellAmount);
    }

    private void rewardHabbo(Habbo habbo, SellableItem item, int amount) {
        habbo.giveCredits(item.getCredits() * amount);
        habbo.givePoints(item.getCurrencyType(), item.getCurrencyAmount() * amount);
    }

    private void deleteHabboItems(Habbo owner, List<HabboItem> items) {
        for (HabboItem item : items) {
            owner.getInventory().getItemsComponent().removeHabboItem(item.getId());
            item.needsDelete(true);
            Emulator.getThreading().run(item);
            owner.getClient().sendResponse(new RemoveHabboItemComposer(item.getId()));
        }
    }
    public Optional<SellableItem> getItemByName(String name) {
        for (SellableItem item : this.sellableItems.values()) {
            if (item.getName().equalsIgnoreCase(name)) {
                return Optional.of(item);
            }
        }
        return Optional.empty();
    }

    public Optional<Collection<HabboItem>> getHabboItemsBySellable(Habbo habbo, SellableItem item) {
        Collection<HabboItem> items = habbo.getInventory().getItemsComponent().getItems().valueCollection().stream().filter(habboItem -> habboItem.getBaseItem().getId() == item.getBaseItem()).collect(Collectors.toList());

        if (items.isEmpty()) {
            return Optional.empty();
        }

        return Optional.of(items);
    }
}
