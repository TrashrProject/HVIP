package io.github.brenoepics.roleplay.commands.staff;

import static io.github.brenoepics.roleplay.features.organizations.OrganizationManager.getOrganizationType;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;

public class MakeTerritoryCommand extends Command {
    public MakeTerritoryCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] strings) {
        if (strings.length != 2) {
            gameClient.getHabbo().whisper("Syntaxe : :creerterritoire <gang/mafia/cartel/tous>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Room room = gameClient.getHabbo().getHabboInfo().getCurrentRoom();
        if (RolePlay.getOrganizationManager().getOrganizationTerritories().containsKey(room.getId())) {
            gameClient.getHabbo().whisper("Cette salle est d\u00e9j\u00e0 un territoire.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        OrganizationType type = getOrganizationType(strings[1]);
        if (type == null) {
            gameClient.getHabbo().whisper("Type invalide : gang, mafia, cartel ou tous.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean isCreated = RolePlay.getOrganizationManager().createTerritory(type, gameClient.getHabbo().getHabboInfo().getCurrentRoom().getId());
        gameClient.getHabbo().whisper(isCreated ? "Le territoire a \u00e9t\u00e9 cr\u00e9\u00e9." : "Impossible de cr\u00e9er le territoire.", RoomChatMessageBubbles.ALERT);
        return true;
    }
}
