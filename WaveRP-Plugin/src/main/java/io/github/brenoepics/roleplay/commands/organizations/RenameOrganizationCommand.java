package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class RenameOrganizationCommand extends Command {

    public RenameOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 2) {
            gameClient.getHabbo().whisper(":rename <name>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());


        if (data.getOrganizationId() == 0) {
            gameClient.getHabbo().whisper("You are not in an organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        StringBuilder name = new StringBuilder();

        for (int i = 1; i < params.length; i++) {
            name.append(params[i]).append(" ");
        }

        Organization org = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (org == null || org.getAdminId() != gameClient.getHabbo().getHabboInfo().getId()) {
            gameClient.getHabbo().whisper("You do not own an organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getOrganizationManager().changeName(org.getId(), name.toString());
        gameClient.getHabbo().whisper("You have changed the name of the organization to " + name, RoomChatMessageBubbles.ALERT);
        return true;
    }
}
