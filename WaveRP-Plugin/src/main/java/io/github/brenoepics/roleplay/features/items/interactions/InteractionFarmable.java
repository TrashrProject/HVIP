package io.github.brenoepics.roleplay.features.items.interactions;

import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.farm.data.FarmItem;
import io.github.brenoepics.roleplay.features.farm.data.ItemState;
import io.github.brenoepics.roleplay.features.farm.data.RewardItem;
import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomLayout;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.outgoing.inventory.RemoveHabboItemComposer;
import com.eu.habbo.messages.outgoing.rooms.UpdateStackHeightComposer;
import com.eu.habbo.messages.outgoing.rooms.items.RemoveFloorItemComposer;
import gnu.trove.set.hash.THashSet;

import java.awt.*;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.List;
import java.util.stream.Collectors;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class InteractionFarmable extends InteractionDefault {

    private static final Logger log = LoggerFactory.getLogger(InteractionFarmable.class);

    public InteractionFarmable(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public InteractionFarmable(int id, int userId, Item item, String extradata, int limitedStack, int limitedSells) {
        super(id, userId, item, extradata, limitedStack, limitedSells);
    }

    @Override
    public void onPickUp(Room room) {
        this.setExtradata(String.valueOf(0));
        this.needsUpdate(true);
        super.onPickUp(room);
    }

    @Override
    public boolean isUsable() {
        return true;
    }

    @Override
    public boolean allowWiredResetState() {
        return false;
    }

    @Override
    public void onClick(GameClient client, Room room, Object[] objects) {
        if (client.getHabbo() == null || room == null) return;

        FarmItem item = RolePlay.getFarmManager().getItemByFurni(this.getBaseItem().getId());
        if (item == null) {
            log.error("Could not find item with id: {}", this.getBaseItem().getId());
            return;
        }

        if (!isUserAllowed(item, room, client.getHabbo()) || this.getBaseItem().getStateCount() <= 0) {
            return;
        }

        int currentState = parseExtraData(this.getExtradata());
        ItemState state = item.getState(currentState);

        if (state == null) {
            log.error("Could not find state with id: {}", currentState);
            return;
        }

        if (RolePlay.getFarmManager().isOnCooldown(this.getId())) {
            client.getHabbo().whisper(Emulator.getTexts().getValue("farm.warn.cooldown").replace("%time%", RolePlay.getFarmManager().getCooldown(this.getId()).toString()));
            return;
        }

        if (state.getEnable() > 0 && state.getEnable() != client.getHabbo().getRoomUnit().getEffectId()) {
            client.getHabbo().whisper(Emulator.getTexts().getValue("farm.warn.missing_effect_" + state.getEnable(), Emulator.getTexts().getValue("farm.warn.missing_effect")).replace("%effect%", String.valueOf(state.getEnable())));
            return;
        }

        if (state.getRequiredItem() > 0 && !takeItem(client.getHabbo(), state)) {
            return;
        }

        RolePlay.getFarmManager().setCooldown(this.getId(), state.getCooldown());

        if (item.getStates().size() > state.getId() + 1) {
            currentState = (currentState + 1) % item.getStates().size();
            updateItemState(room, currentState);
            return;
        }

        giveRewardList(item.getRandomRewards(), client.getHabbo());
        currentState = 0;
        client.getHabbo().whisper(Emulator.getTexts().getValue("farm.warn.harvested").replace("%item%", this.getBaseItem().getName()));

        if (item.delete()) {
            deleteItem(room);
            return;
        }

        updateItemState(room, currentState);
    }

    private boolean takeItem(Habbo habbo, ItemState state) {
        List<HabboItem> itemsStream = getHabboItemsByBase(habbo, state.getRequiredItem());
        if (!listHasFurni(itemsStream, state.getRequiredAmount())) {
            Item hItem = Emulator.getGameEnvironment().getItemManager().getItem(state.getRequiredItem());
            if (hItem == null) {
                log.error("Could not find item with id: {}", state.getRequiredItem());
                return false;
            }

            habbo.whisper(Emulator.getTexts().getValue("farm.warn.missing_item").replace("%item%", hItem.getName()).replace("%amount%", String.valueOf(state.getRequiredAmount())));
            return false;
        }

        int removedItems = 0;
        for (HabboItem item : itemsStream) {
            if (removedItems >= state.getRequiredAmount()) break;
            habbo.getInventory().getItemsComponent().removeHabboItem(item.getId());
            item.needsDelete(true);
            Emulator.getThreading().run(item);
            habbo.getClient().sendResponse(new RemoveHabboItemComposer(item.getId()));
            removedItems++;
        }
        return removedItems >= state.getRequiredAmount();
    }

    private boolean listHasFurni(List<HabboItem> items, int amount) {
        return items.size() >= amount;
    }

    private List<HabboItem> getHabboItemsByBase(Habbo habbo, int itemId) {
        return habbo.getInventory().getItemsComponent().getItems().valueCollection().stream().filter(item -> item.getBaseItem().getId() == itemId).collect(Collectors.toList());
    }

    private void deleteItem(Room room) {
        room.removeHabboItem(this);
        room.sendComposer((new RemoveFloorItemComposer(this, true)).compose());
        this.setRoomId(0);
        Emulator.getGameEnvironment().getItemManager().deleteItem(this);
        THashSet<RoomTile> updatedTiles = getTiles(room.getLayout());

        room.sendComposer(new UpdateStackHeightComposer(room, updatedTiles).compose());
        room.updateTiles(updatedTiles);
        for (RoomTile tile : updatedTiles) {
            room.updateHabbosAt(tile.x, tile.y);
            room.updateBotsAt(tile.x, tile.y);
        }

    }

    private THashSet<RoomTile> getTiles(RoomLayout layout) {
        THashSet<RoomTile> tiles = new THashSet<>();
        Rectangle rectangle = RoomLayout.getRectangle(this.getX(), this.getY(), this.getBaseItem().getWidth(), this.getBaseItem().getLength(), this.getRotation());

        for (short x = (short) rectangle.x; x < rectangle.x + rectangle.getWidth(); x++) {
            for (short y = (short) rectangle.y; y < rectangle.y + rectangle.getHeight(); y++) {
                RoomTile tile = layout.getTile(x, y);
                if (tile != null) {
                    tiles.add(tile);
                }
            }
        }

        return tiles;
    }

    private void giveRewardList(List<Integer> random, Habbo habbo) {
        for (Integer i : random) {
            RewardItem reward = RolePlay.getFarmManager().getReward(i);
            if (reward == null) {
                log.error("Could not find random reward for item ID ({}) of type ({}})", this.getId(), this.getBaseItem().getName());
                return;
            }
            reward.give(habbo);
            if (Emulator.getConfig().getBoolean("farm.log_rewards", true))
                RolePlay.getFarmManager().log(this.getId(), habbo.getHabboInfo().getId(), getRoomId(), i);

        }
    }

    private void updateItemState(Room room, int currentState) {
        this.setExtradata(String.valueOf(currentState));
        this.needsUpdate(true);
        room.updateItemState(this);
    }

    private boolean isUserAllowed(FarmItem item, Room room, Habbo habbo) {
        if (item.isOwnerOnly() && this.getUserId() != habbo.getHabboInfo().getId()) return false;
        return this.getOccupyingTiles(room.getLayout()).stream().noneMatch(rt -> rt.distance(habbo.getRoomUnit().getCurrentLocation()) > 1);
    }

    private int parseExtraData(String extraData) {
        try {
            return Integer.parseInt(extraData);
        } catch (NumberFormatException e) {
            log.error("Incorrect extradata ({}}) for item ID ({}}) of type ({}})", extraData, this.getId(), this.getBaseItem().getName());
            return 0;
        }
    }
}
