package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class CreateOrganizationCommand extends Command {

    public CreateOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 3) {
            gameClient.getHabbo().whisper(":creerorganisation <gang/mafia/cartel> <nom>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (data.getOrganizationId() != 0) {
            gameClient.getHabbo().whisper("Vous appartenez d\u00e9j\u00e0 une organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        OrganizationType type;
        try {
            type = OrganizationType.valueOf(params[1].toUpperCase());
            if (type == OrganizationType.ANY) {
                throw new IllegalArgumentException();
            }
        } catch (IllegalArgumentException e) {
            gameClient.getHabbo().whisper("Type d'organisation invalide : gang, mafia ou cartel.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        StringBuilder name = new StringBuilder();

        for (int i = 2; i < params.length; i++) {
            name.append(params[i]).append(" ");
        }

        if (name.toString().length() > 15) {
            gameClient.getHabbo().whisper("Le nom de l'organisation ne peut pas d\u00e9passer 15 caract\u00e8res.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOrganizationManager().getOrganization(name.toString()) != null) {
            gameClient.getHabbo().whisper("Une organisation porte d\u00e9j\u00e0 ce nom.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if(gameClient.getHabbo().getHabboInfo().getCurrencyAmount(200) < Emulator.getConfig().getInt("features.organizations."+ type.name().toLowerCase() +".price", 20)) {
            gameClient.getHabbo().whisper("Vous n'avez pas assez d'argent pour cr\u00e9er cette organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean isCreated = RolePlay.getOrganizationManager().createOrganization(gameClient.getHabbo().getHabboInfo().getId(), name.toString(), type);
        if (isCreated) {
            gameClient.getHabbo().getHabboInfo().addCurrencyAmount(200, -Emulator.getConfig().getInt("features.organizations."+ type.name().toLowerCase() +".price", 20));
            gameClient.getHabbo().whisper("Vous avez cr\u00e9\u00e9 l'organisation " + type.name().toLowerCase() + " " + name + ".", RoomChatMessageBubbles.ALERT);
            return true;
        }

        gameClient.getHabbo().whisper("Une erreur est survenue pendant la cr\u00e9ation de l'organisation.", RoomChatMessageBubbles.ALERT);
        return true;
    }
}
