package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;

public class SetZCommand extends Command {
    private static final double MIN_HEIGHT = -30.0D;
    private static final double MAX_HEIGHT = 100.0D;

    public SetZCommand() {
        super(null, new String[]{"setz"});
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        Room room = gameClient.getHabbo().getHabboInfo().getCurrentRoom();
        if (room == null) {
            return true;
        }

        boolean canBuild = room.hasRights(gameClient.getHabbo())
                || gameClient.getHabbo().getHabboInfo().getRank().getLevel() > 1;

        if (!canBuild) {
            gameClient.getHabbo().whisper("Tu n'as pas la permission d'utiliser :setz ici.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length < 2) {
            gameClient.getHabbo().whisper("Utilisation : :setz <hauteur> ou :setz stop", RoomChatMessageBubbles.ALERT);
            return true;
        }

        String value = params[1].trim();
        if (value.equalsIgnoreCase("stop") || value.equalsIgnoreCase("off")) {
            ParadiseBuildState.setForcedZ(gameClient.getHabbo(), null);
            gameClient.getHabbo().whisper("SetZ désactivé. La hauteur automatique est rétablie.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        final double height;
        try {
            height = Double.parseDouble(value.replace(',', '.'));
        } catch (NumberFormatException exception) {
            gameClient.getHabbo().whisper("Hauteur invalide. Exemple : :setz 2.5", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!Double.isFinite(height) || height < MIN_HEIGHT || height > MAX_HEIGHT) {
            gameClient.getHabbo().whisper("La hauteur doit être comprise entre -30 et 100.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        ParadiseBuildState.setForcedZ(gameClient.getHabbo(), height);
        gameClient.getHabbo().whisper("SetZ activé à " + formatHeight(height) + ". :setz stop pour désactiver.", RoomChatMessageBubbles.ALERT);
        return true;
    }

    private static String formatHeight(double height) {
        if (height == Math.rint(height)) {
            return Integer.toString((int) height);
        }
        return Double.toString(height);
    }
}
