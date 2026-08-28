package io.github.brenoepics.roleplay.commands.jobs;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class DemoteCommand extends Command {

    public DemoteCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (data.getJobEntity() == null || data.getJobEntity().equals(RolePlay.getJobService().getUnemployedJob())) {
            gameClient.getHabbo().whisper("Vous n'avez aucun m\u00e9tier.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length != 2) {
            gameClient.getHabbo().whisper(":retrograder <pseudo>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            gameClient.getHabbo().whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.isDuty()) {
            gameClient.getHabbo().whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RolePlay.getJobsManager().demoteUser(gameClient.getHabbo(), data, habbo);
        data.executeAction();
        return true;
    }
}
