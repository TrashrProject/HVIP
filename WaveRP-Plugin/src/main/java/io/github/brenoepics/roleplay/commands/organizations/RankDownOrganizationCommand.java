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
            gameClient.getHabbo().whisper(":retrograderorganisation <pseudo>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());


        if (data.getOrganizationId() == 0) {
            return true;
        }

        Organization org = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (org == null || org.getAdminId() != gameClient.getHabbo().getHabboInfo().getId()) {
            gameClient.getHabbo().whisper("Vous ne pouvez pas r\u00e9trograder ce joueur.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        HabboInfo target;
        Habbo targetUser = Emulator.getGameEnvironment().getHabboManager().getHabbo(params[1]);
        target = targetUser == null ? HabboManager.getOfflineHabboInfo(params[1]) : targetUser.getHabboInfo();
        if (target == null) {
            gameClient.getHabbo().whisper("Ce joueur est introuvable.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Integer userOrg = RolePlay.getOrganizationManager().getUserOrganization(target.getId());
        if (userOrg == null || userOrg != org.getId()) {
            gameClient.getHabbo().whisper("Ce joueur n'appartient pas \u00e0 votre organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean success = RolePlay.getOrganizationManager().rankDownUser(target.getId(), org.getId());
        if (!success) {
            gameClient.getHabbo().whisper("Ce joueur poss\u00e8de d\u00e9j\u00e0 le grade minimal.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        gameClient.getHabbo().whisper("Vous avez r\u00e9trograd\u00e9 " + target.getUsername() + ".", RoomChatMessageBubbles.ALERT);
        return true;
    }
}
