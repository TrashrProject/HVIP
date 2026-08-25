package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class LeaveOrganizationCommand extends Command {

    public LeaveOrganizationCommand(String permission, String[] keys) {
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
        if (org != null && org.getAdminId() == gameClient.getHabbo().getHabboInfo().getId()) {
            gameClient.getHabbo().whisper("Vous devez transmettre la propri\u00e9t\u00e9 ou dissoudre l'organisation avant de la quitter.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if(org == null) return true;
        RolePlay.getOrganizationManager().removeMember(org.getId(), gameClient.getHabbo().getHabboInfo().getId());
        data.setOrganizationId(0);
        gameClient.getHabbo().whisper("Vous avez quitt\u00e9 l'organisation.", RoomChatMessageBubbles.ALERT);

        return true;
    }
}
