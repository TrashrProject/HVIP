package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class DisbandOrganizationCommand extends Command {

    public DisbandOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (data.getOrganizationId() == 0) {
            gameClient.getHabbo().whisper("Vous n'appartenez \u00e0 aucune organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Organization org = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (org == null || org.getAdminId() != gameClient.getHabbo().getHabboInfo().getId()) {
            gameClient.getHabbo().whisper("Vous n'\u00eates pas propri\u00e9taire d'une organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if(params.length < 2 || !params[1].equals("confirm")) {
            gameClient.getHabbo().whisper("Cette action supprimera d\u00e9finitivement votre organisation. Confirmez avec :supprimerorganisation confirmer", RoomChatMessageBubbles.ALERT);
            return true;
        }


        RolePlay.getOrganizationManager().disbandOrganization(org.getId());
        gameClient.getHabbo().whisper("Votre organisation a \u00e9t\u00e9 dissoute.", RoomChatMessageBubbles.ALERT);
        return true;
    }
}
