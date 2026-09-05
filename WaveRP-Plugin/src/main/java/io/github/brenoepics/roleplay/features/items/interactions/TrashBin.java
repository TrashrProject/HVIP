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
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Arrays;

/**
 * Interaction RP des poubelles.
 *
 * Le loot est genere exclusivement cote serveur. Le verrou et le cooldown sont portes par
 * l'instance du mobilier, ce qui empeche plusieurs joueurs de farmer la meme poubelle.
 */
public class TrashBin extends InteractionDefault {

    private volatile boolean occupied = false;
    private volatile boolean searched = false;

    public TrashBin(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public TrashBin(int id, int userId, Item item, String extradata, int limitedStack,
                    int limitedSells) {
        super(id, userId, item, extradata, limitedStack, limitedSells);
    }

    @Override
    public boolean isUsable() {
        return true;
    }

    @Override
    public void onClick(final GameClient client, final Room room, Object[] objects) {
        if (client == null || client.getHabbo() == null || room == null) {
            return;
        }

        Habbo habbo = client.getHabbo();
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
        if (data == null || habbo.getRoomUnit() == null) {
            return;
        }

        // Hors des salles RP, conserver exactement le toggle natif Arcturus du mobilier.
        if (room.getCategory() != Emulator.getConfig().getInt("nahabbo.features.room.category")) {
            toggleLikeNormalFurniture(room);
            return;
        }

        if (data.isDead()) {
            return;
        }

        if (!isHabboStillInRoom(habbo, room) || !isAdjacent(habbo, room)) {
            habbo.whisper("Vous devez être à côté de la poubelle pour la fouiller.");
            return;
        }

        synchronized (this) {
            if (searched) {
                habbo.whisper("Cette poubelle a déjà été fouillée récemment.");
                return;
            }
            if (occupied) {
                habbo.whisper("Quelqu'un est déjà en train de fouiller cette poubelle.");
                return;
            }
            occupied = true;
        }

        LootTable lootTable = readLootTable();
        if (lootTable == null) {
            occupied = false;
            habbo.whisper("Cette poubelle ne peut pas être fouillée pour le moment.");
            return;
        }

        room.sendComposer(new RoomUserShoutComposer(new RoomChatMessage(
            "* Fouille la poubelle... *",
            habbo,
            habbo,
            RoomChatMessageBubbles.NORMAL)).compose());

        // Ouvrir avec le MEME chemin que l'utilisation normale d'un mobi Arcturus.
        // On repart volontairement de 0 afin que le toggle natif arrive sur l'etat 1.
        openLikeNormalFurniture(room);

        final int habboId = habbo.getHabboInfo().getId();
        final int searchDelay = Math.max(1000,
            Emulator.getConfig().getInt("nahabbo.features.trashbin.search.time"));

        Emulator.getThreading().run(
            () -> finishSearch(habbo, habboId, data, room, lootTable),
            searchDelay);
    }

    private void finishSearch(Habbo habbo, int habboId, RpAvatar data, Room room,
                              LootTable lootTable) {
        try {
            if (habbo == null || habbo.getHabboInfo().getId() != habboId
                || !isHabboStillInRoom(habbo, room) || !isAdjacent(habbo, room)) {
                if (habbo != null) {
                    habbo.whisper("La fouille a été annulée : vous vous êtes éloigné de la poubelle.");
                }
                closeFurniture(room);
                return;
            }

            String rewardName = roll(lootTable);
            giveReward(rewardName, habbo, data);

            synchronized (this) {
                searched = true;
            }

            final int cooldown = Math.max(1000,
                Emulator.getConfig().getInt("nahabbo.features.trashbin.cooldown"));
            Emulator.getThreading().run(() -> {
                synchronized (TrashBin.this) {
                    searched = false;
                }
                closeFurniture(room);
            }, cooldown);
        } finally {
            occupied = false;
        }
    }

    /** Reproduit le toggle natif d'InteractionDefault sans exiger les droits de la salle. */
    private void toggleLikeNormalFurniture(Room room) {
        if (room == null) {
            return;
        }
        try {
            super.onClick(null, room, new Object[]{0});
        } catch (Exception ignored) {
            // Le systeme RP ne doit jamais casser la fouille si un mobi atypique refuse le toggle.
        }
    }

    private void openLikeNormalFurniture(Room room) {
        if (room == null) {
            return;
        }
        closeFurniture(room);
        toggleLikeNormalFurniture(room);
    }

    private void closeFurniture(Room room) {
        if (room == null) {
            return;
        }
        setExtradata("0");
        needsUpdate(true);
        room.updateItemState(this);
    }

    private LootTable readLootTable() {
        String rawItems = Emulator.getConfig().getValue("nahabbo.features.trashbin.items");
        String rawChances = Emulator.getConfig().getValue("nahabbo.features.trashbin.chances");
        if (rawItems == null || rawItems.isBlank() || rawChances == null || rawChances.isBlank()) {
            return null;
        }

        String[] items = Arrays.stream(rawItems.split(","))
            .map(String::trim)
            .toArray(String[]::new);

        int[] chances;
        try {
            chances = Arrays.stream(rawChances.split(","))
                .map(String::trim)
                .mapToInt(Integer::parseInt)
                .toArray();
        } catch (NumberFormatException ex) {
            return null;
        }

        if (items.length == 0 || items.length != chances.length) {
            return null;
        }

        int total = 0;
        for (int chance : chances) {
            if (chance < 0) {
                return null;
            }
            total += chance;
        }

        return total > 0 ? new LootTable(items, chances, total) : null;
    }

    private String roll(LootTable lootTable) {
        int random = Emulator.getRandom().nextInt(lootTable.total()) + 1;
        for (int i = 0; i < lootTable.chances().length; i++) {
            random -= lootTable.chances()[i];
            if (random <= 0) {
                return lootTable.items()[i];
            }
        }
        return "Nothing";
    }

    private void giveReward(String rewardName, Habbo habbo, RpAvatar data) {
        if (rewardName == null || rewardName.isBlank()
            || rewardName.equalsIgnoreCase("Nothing")
            || rewardName.equalsIgnoreCase("Rien")) {
            habbo.whisper("* Fouille la poubelle mais ne trouve rien. *");
            return;
        }

        RPItem reward = RolePlay.getItemManager().getItemByName(rewardName);
        if (reward == null) {
            habbo.whisper("* Fouille la poubelle mais ne trouve rien. *");
            return;
        }

        data.getInventory().addItem(habbo, reward, 1);
        habbo.whisper("* Fouille la poubelle et trouve : " + reward.getDisplayName() + " *");
    }

    private boolean isHabboStillInRoom(Habbo habbo, Room room) {
        return habbo.getHabboInfo().getCurrentRoom() == room && habbo.getRoomUnit() != null;
    }

    private boolean isAdjacent(Habbo habbo, Room room) {
        RoomTile location = room.getLayout().getTile(getX(), getY());
        if (location == null || habbo.getRoomUnit() == null
            || habbo.getRoomUnit().getCurrentLocation() == null) {
            return false;
        }
        return room.getLayout().getTilesAround(location, 0, false)
            .contains(habbo.getRoomUnit().getCurrentLocation());
    }

    @Override
    public void onPickUp(Room room) {
        synchronized (this) {
            occupied = false;
            searched = false;
        }
        closeFurniture(room);
    }

    private record LootTable(String[] items, int[] chances, int total) {
    }
}
