package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.OrganizationMember;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.List;

public class InviteOrganizationCommand extends Command {

    public InviteOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 2) {
            gameClient.getHabbo().whisper(":recruit <username>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (data.getOrganizationId() == 0) {
            gameClient.getHabbo().whisper("You are not in an organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        int userId = gameClient.getHabbo().getHabboInfo().getId();
        Organization org = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (org == null) {
            gameClient.getHabbo().whisper("You are not an admin of your organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        List<OrganizationMember> members = org.getMembers();

        if(members == null || members.isEmpty() || members.stream().noneMatch(member -> member.getUserId() == userId && member.getRank().isAdministrator())) {
            gameClient.getHabbo().whisper("You are not an admin of your organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo target = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (target == null) {
            gameClient.getHabbo().whisper("This user is not in the room!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!RolePlay.getOrganizationManager().inviteUser(target.getHabboInfo().getId(), org.getId())) {
            gameClient.getHabbo().whisper("This user is already in an organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        gameClient.getHabbo().whisper("You have invited " + params[1] + " to the organization!", RoomChatMessageBubbles.ALERT);
        String message = gameClient.getHabbo().getHabboInfo().getUsername() + " has invited you to join the organization " + org.getName() + "!";
        target.whisper(message, RoomChatMessageBubbles.ALERT);
        //OfferComposer offerComposer = new OfferComposer(message, 0);
        //target.getClient().sendResponse(new JavascriptCallbackComposer(offerComposer));

        return true;
    }
}
