package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.concurrent.ConcurrentHashMap;

/**
 * ParadiseRP coffee machine.
 *
 * <p>The machine charges three wallet credits and restores up to ten energy points. It reuses
 * the existing RpAvatar energy stat and Habbo wallet; no parallel economy/stat is introduced.</p>
 */
public class InteractionCoffeeMachine extends InteractionDefault {

    private static final int PRICE = 3;
    private static final int ENERGY_RESTORE = 10;
    private static final int MAX_INTERACTION_DISTANCE = 2;
    private static final long CLICK_COOLDOWN_MS = 1500L;
    private static final ConcurrentHashMap<Integer, Long> LAST_USE = new ConcurrentHashMap<>();

    public InteractionCoffeeMachine(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public InteractionCoffeeMachine(int id, int userId, Item item, String extradata,
                                    int limitedStack, int limitedSells) {
        super(id, userId, item, extradata, limitedStack, limitedSells);
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
        if (client == null || client.getHabbo() == null || room == null) {
            return;
        }

        Habbo habbo = client.getHabbo();
        RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
        if (avatar == null || avatar.isDead()) {
            return;
        }

        if (!isUserInRange(habbo, room)) {
            habbo.whisper("Vous devez vous rapprocher de la machine à café.", RoomChatMessageBubbles.ALERT);
            return;
        }

        int userId = habbo.getHabboInfo().getId();
        long now = System.currentTimeMillis();
        Long previous = LAST_USE.get(userId);
        if (previous != null && now - previous < CLICK_COOLDOWN_MS) {
            return;
        }

        if (avatar.getEnergy() >= avatar.getMaxEnergy()) {
            habbo.whisper("Votre énergie est déjà au maximum.", RoomChatMessageBubbles.ALERT);
            return;
        }

        int energyBefore = avatar.getEnergy();
        int energyAfter = Math.min(energyBefore + ENERGY_RESTORE, avatar.getMaxEnergy());
        int energyGained = energyAfter - energyBefore;

        synchronized (habbo) {
            if (habbo.getHabboInfo().getCredits() < PRICE) {
                habbo.whisper("Vous n'avez pas assez de crédits pour acheter un café (3 crédits).",
                        RoomChatMessageBubbles.ALERT);
                return;
            }

            LAST_USE.put(userId, now);
            habbo.giveCredits(-PRICE);
            avatar.setEnergy(energyAfter);
            avatar.updateClientData();
            avatar.updateDatabase();
        }

        habbo.shout("* Achète un café pour 3 crédits *", RoomChatMessageBubbles.NORMAL);

        int roomId = room.getId();
        Emulator.getThreading().run(() -> {
            Habbo currentHabbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
            if (currentHabbo == null || currentHabbo.getClient() == null
                    || currentHabbo.getHabboInfo().getCurrentRoom() == null
                    || currentHabbo.getHabboInfo().getCurrentRoom().getId() != roomId) {
                return;
            }

            currentHabbo.shout(
                    "* Boit son café (+" + energyGained + " énergie) — Énergie : "
                            + energyAfter + "/" + avatar.getMaxEnergy() + " *",
                    RoomChatMessageBubbles.NORMAL);
        }, 1000L);
    }

    private boolean isUserInRange(Habbo habbo, Room room) {
        if (habbo.getRoomUnit() == null) {
            return false;
        }

        RoomTile userTile = habbo.getRoomUnit().getCurrentLocation();
        RoomTile machineTile = room.getLayout().getTile((short) this.getX(), (short) this.getY());
        if (userTile == null || machineTile == null) {
            return false;
        }

        double distance = Math.sqrt(
                Math.pow(userTile.x - machineTile.x, 2) + Math.pow(userTile.y - machineTile.y, 2));
        return distance <= MAX_INTERACTION_DISTANCE;
    }
}
