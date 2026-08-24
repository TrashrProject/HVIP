package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.OrganizationRank;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class JoinOrganizationCommand extends Command {

    public JoinOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 2) {
            gameClient.getHabbo().whisper(":join <name>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());


        if (data.getOrganizationId() != 0) {
            gameClient.getHabbo().whisper("You are already in an organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        StringBuilder name = new StringBuilder();

        for (int i = 1; i < params.length; i++) {
            name.append(params[i]).append(" ");
        }

        Organization org = RolePlay.getOrganizationManager().getOrganization(name.toString());
        if (org == null) {
            gameClient.getHabbo().whisper("This organization does not exist!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if(!RolePlay.getOrganizationManager().isInvited(gameClient.getHabbo().getHabboInfo().getId(), org.getId())) {
            gameClient.getHabbo().whisper("You are not invited to this organization! ask the owner to invite you!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getOrganizationManager().addMember(gameClient.getHabbo().getHabboInfo().getId(), org.getId(), OrganizationRank.MEMBER.getId());
        data.setOrganizationId(org.getId());
        gameClient.getHabbo().whisper("You have joined the organization!", RoomChatMessageBubbles.ALERT);
        return true;
    }
}
