package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.permissions.Permission;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomState;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

public class TaxiCommand extends Command {
    private static final int COST = 3;
    private static final int DELAY_SECONDS = 10;
    private static final int DEFAULT_TAXI_EFFECT = 21;
    private static final Set<Integer> PENDING = ConcurrentHashMap.newKeySet();

    public TaxiCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        Habbo habbo = gameClient.getHabbo();
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
        Room origin = habbo.getHabboInfo().getCurrentRoom();

        if (data == null || origin == null) return true;
        if (data.isDead() || data.isJailed()) {
            habbo.whisper("Tu ne peux pas appeler un taxi lorsque tu es mort ou en prison!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length != 2) {
            habbo.whisper("Utilisation : :taxi [ID de la salle]", RoomChatMessageBubbles.ALERT);
            return true;
        }

        int roomId;
        try {
            roomId = Integer.parseInt(params[1]);
        } catch (NumberFormatException e) {
            habbo.whisper("L'ID de la salle doit etre un nombre.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Room room = Emulator.getGameEnvironment().getRoomManager().loadRoom(roomId);
        if (room == null) {
            habbo.whisper("Cette salle n'existe pas.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (!isAccessible(habbo, room)) {
            habbo.whisper("Cette salle n'est pas accessible.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (origin.getId() == roomId) {
            habbo.whisper("Tu es deja dans cette salle.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (habbo.getHabboInfo().getCredits() < COST) {
            habbo.whisper("Tu n'as pas assez de credits.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (!PENDING.add(habbo.getHabboInfo().getId())) {
            habbo.whisper("Tu as deja appele un taxi.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        int effect = Emulator.getConfig().getInt("features.taxi.effectid", DEFAULT_TAXI_EFFECT);
        habbo.shout("* Appelle le taxi pour se rendre a [" + roomId + "] " + room.getName() + " *", RoomChatMessageBubbles.YELLOW);
        habbo.whisper("Ton taxi arrive dans 10 secondes...");
        origin.giveEffect(habbo, effect, -1);
        Emulator.getThreading().run(() -> complete(habbo.getHabboInfo().getId(), origin.getId(), roomId, effect), DELAY_SECONDS * 1000L);
        return true;
    }

    private static void complete(int userId, int originId, int destinationId, int effect) {
        try {
            Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
            if (habbo == null || habbo.getClient() == null) return;
            Room current = habbo.getHabboInfo().getCurrentRoom();
            if (current == null || current.getId() != originId) return;
            Room destination = Emulator.getGameEnvironment().getRoomManager().loadRoom(destinationId);
            if (destination == null || !isAccessible(habbo, destination)) {
                habbo.whisper("Cette salle n'est plus accessible.", RoomChatMessageBubbles.ALERT);
                return;
            }
            synchronized (habbo) {
                if (habbo.getHabboInfo().getCredits() < COST) {
                    habbo.whisper("Tu n'as pas assez de credits.", RoomChatMessageBubbles.ALERT);
                    return;
                }
                habbo.giveCredits(-COST);
            }
            habbo.goToRoom(destinationId);
        } finally {
            Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
            if (habbo != null && habbo.getRoomUnit() != null && habbo.getRoomUnit().getEffectId() == effect) {
                Room room = habbo.getHabboInfo().getCurrentRoom();
                if (room != null) room.giveEffect(habbo, 0, -1);
            }
            PENDING.remove(userId);
        }
    }

    private static boolean isAccessible(Habbo habbo, Room room) {
        return room.getState() == RoomState.OPEN
            && (!room.isBanned(habbo) || habbo.hasPermission(Permission.ACC_ANYROOMOWNER))
            && (room.getUserCount() < room.getUsersMax() || room.isOwner(habbo)
                || habbo.hasPermission(Permission.ACC_FULLROOMS));
    }
}
