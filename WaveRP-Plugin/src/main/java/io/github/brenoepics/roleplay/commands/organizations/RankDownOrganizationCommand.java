package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import com.eu.habbo.habbohotel.users.HabboManager;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class RankDownOrganizationCommand extends Command {

    public RankDownOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 2) {
            gameClient.getHabbo().whisper(":rankdown <username>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());


        if (data.getOrganizationId() == 0) {
            return true;
        }

        Organization org = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (org == null || org.getAdminId() != gameClient.getHabbo().getHabboInfo().getId()) {
            gameClient.getHabbo().whisper("You cannot rank down this user!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        HabboInfo target;
        Habbo targetUser = Emulator.getGameEnvironment().getHabboManager().getHabbo(params[1]);
        target = targetUser == null ? HabboManager.getOfflineHabboInfo(params[1]) : targetUser.getHabboInfo();
        if (target == null) {
            gameClient.getHabbo().whisper("This user does not exist!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Integer userOrg = RolePlay.getOrganizationManager().getUserOrganization(target.getId());
        if (userOrg == null || userOrg != org.getId()) {
            gameClient.getHabbo().whisper("This user is not in your organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean success = RolePlay.getOrganizationManager().rankDownUser(target.getId(), org.getId());
        if (!success) {
            gameClient.getHabbo().whisper("This user is already the lowest rank!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        gameClient.getHabbo().whisper("You have ranked down " + target.getUsername() + "!", RoomChatMessageBubbles.ALERT);
        return true;
    }
}