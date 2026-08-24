package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Arrays;
import java.util.concurrent.ThreadLocalRandom;

public class TrashBin extends InteractionDefault {

    private boolean occupied = false;
    private boolean searched = false;

    public TrashBin(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public TrashBin(int id, int userId, Item item, String extradata, int limitedStack, int limitedSells) {
        super(id, userId, item, extradata, limitedStack, limitedSells);
    }

    @Override
    public void onClick(final GameClient client, final Room room, Object[] objects) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(client.getHabbo());
        if (room.getCategory() != Emulator.getConfig().getInt("nahabbo.features.room.category")) {
            this.setExtradata(this.getExtradata().isEmpty() ? "0" : String.valueOf((Integer.parseInt(this.getExtradata()) + 1) % this.getBaseItem().getStateCount()));
            this.needsUpdate(true);
            room.updateItemState(this);
            return;
        }

        if (this.occupied || data.isDead() || room.getCategory() != Emulator.getConfig().getInt("nahabbo.features.room.category")) {
            return;
        }

        if (this.searched) {
            client.getHabbo().whisper("This trash can has already been searched, try again later");
            return;
        }

        String[] items = Emulator.getConfig().getValue("nahabbo.features.trashbin.items").split(","); // Items that can be found in the trash can (separate by commas
        int[] chances = Arrays.stream(Emulator.getConfig().getValue("nahabbo.features.trashbin.chances").split(",")).mapToInt(Integer::parseInt).toArray(); // Chances of finding each item (separated by commas)
        int sum = Arrays.stream(chances).sum();
        TrashBin bin = this;
        RoomTile location = room.getLayout().getTile(getX(), getY());
        if (client.getHabbo().getHabboInfo().getCurrentRoom().getLayout().getTilesAround(location, 0, false).contains(client.getHabbo().getRoomUnit().getCurrentLocation())) {
            this.occupied = true;
            room.updateItem(this);
            client.getHabbo().getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(new RoomChatMessage("Searches the trash can for some useful items*", client.getHabbo(), client.getHabbo(), RoomChatMessageBubbles.NORMAL)).compose());
            Emulator.getThreading().run(() -> updateOccupied(client.getHabbo(), sum, chances, items, data, room, bin, location), Emulator.getConfig().getInt("nahabbo.features.trashbin.search.time"));
        }
    }

    private void updateOccupied(Habbo habbo, int sum, int[] chances, String[] items, RpAvatar data, Room room, TrashBin bin, RoomTile location) {
        if (habbo == null) return;

        int random = Emulator.getRandom().nextInt(sum) + 1;
        int i = 0;
        while (random > chances[i]) {
            random -= chances[i];
            i++;
        }
        if (habbo.getHabboInfo().getCurrentRoom().getLayout().getTilesAround(location, 0, false).contains(habbo.getRoomUnit().getCurrentLocation())) {
            handleSearch(items[i], habbo, data);
            this.searched = true;
            bin.setExtradata("1");
            room.updateItem(bin);
            Emulator.getThreading().run(() -> {
                bin.setExtradata("2");
                room.updateItem(bin);
                bin.searched = false;
            }, Emulator.getConfig().getInt("nahabbo.features.trashbin.cooldown"));
        }
        this.occupied = false;
    }

    private void handleSearch(String item, Habbo habbo, RpAvatar data) {
        switch (item) {
            case "Bucks":
                int bucks = ThreadLocalRandom.current().nextInt(1, 5);
                habbo.getHabboInfo().addCurrencyAmount(200, bucks);
                habbo.whisper("You found " + bucks + " Bucks");
                break;
            case "Pizza":
                data.getInventory().addItem(habbo, RolePlay.getItemManager().getItemByName("Snack"), 1);
                habbo.whisper("You found a Snack");
                break;
            case "Medkit":
                data.getInventory().addItem(habbo, RolePlay.getItemManager().getItemByName("Medkit"), 1);
                habbo.whisper("You found a Medkit");
                break;
            case "Shield":
                data.getInventory().addItem(habbo, RolePlay.getItemManager().getItemByName("Shield"), 1);
                habbo.whisper("You found a Shield");
                break;
            case "Weapon":
                int weapon = ThreadLocalRandom.current().nextInt(1, 3);
                if (weapon == 1) {
                    data.getInventory().addItem(habbo, RolePlay.getItemManager().getItemByName("Bat"), 1);
                    habbo.whisper("You found a Bat");
                    return;
                }
                if (weapon == 2) {
                    data.getInventory().addItem(habbo, RolePlay.getItemManager().getItemByName("Sword"), 1);
                    habbo.whisper("You found a Sword");
                    return;
                }
                if (weapon == 3) {
                    data.getInventory().addItem(habbo, RolePlay.getItemManager().getItemByName("Pistol"), 1);
                    habbo.whisper("You found a Pistol");
                }
                break;
            default:
                habbo.whisper("You found nothing");
                break;
        }
    }

    @Override
    public void onPickUp(Room room) {
        this.occupied = false;
    }
}
