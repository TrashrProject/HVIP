package io.github.brenoepics.roleplay.commands.jobs;

import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class PromoteCommand extends Command {

    public PromoteCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        Habbo habbo1 = gameClient.getHabbo();
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo1);

        if (data.getJobEntity() == null || data.getJobEntity().equals(RolePlay.getJobService().getUnemployedJob())) {
            habbo1.whisper("Vous n'avez aucun m\u00e9tier.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length != 2) {
            habbo1.whisper(":promouvoir <pseudo>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.hasEnergy()) {
            habbo1.whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = habbo1.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            habbo1.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.isDuty()) {
            gameClient.getHabbo().whisper("Vous devez \u00eatre en service.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getJobsManager().promoteUser(habbo1, data, habbo);
        data.executeAction();
        return true;
    }
}
