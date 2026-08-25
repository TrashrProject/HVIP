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
import io.github.brenoepics.roleplay.features.organizations.OrganizationMember;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.List;

public class KickOrganizationCommand extends Command {

    public KickOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 2) {
            gameClient.getHabbo().whisper(":exclureorganisation <pseudo>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());


        if (data.getOrganizationId() == 0) {
            return true;
        }

        Organization org = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (org == null) {
            gameClient.getHabbo().whisper("Vous n'\u00eates pas propri\u00e9taire d'une organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        List<OrganizationMember> members = org.getMembers();
        if (members == null || members.isEmpty() || members.stream().noneMatch(member -> member.getUserId() == gameClient.getHabbo().getHabboInfo().getId() && member.getRank().isAdministrator())) {
            gameClient.getHabbo().whisper("Vous n'\u00eates pas administrateur de cette organisation.", RoomChatMessageBubbles.ALERT);
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

        RolePlay.getOrganizationManager().kickUser(target.getId(), org.getId());
        if (targetUser != null && RolePlay.getAvatarManager().getCachedData().containsKey(targetUser)) {
            RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(targetUser);
            targetData.setOrganizationId(0);
        }
        gameClient.getHabbo().whisper("Vous avez exclu " + params[1] + " de l'organisation.", RoomChatMessageBubbles.ALERT);

        return true;
    }
}
