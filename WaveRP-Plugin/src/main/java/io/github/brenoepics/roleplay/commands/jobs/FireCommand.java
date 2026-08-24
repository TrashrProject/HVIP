package io.github.brenoepics.roleplay.commands.jobs;

import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class FireCommand extends Command {

    public FireCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (data.getJobEntity() == null || data.getJobEntity().equals(RolePlay.getJobService().getUnemployedJob())) {
            gameClient.getHabbo().whisper("You are not employed", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length != 2) {
            gameClient.getHabbo().whisper(":fire <user>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.hasEnergy()) {
            gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            gameClient.getHabbo().whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!data.isDuty()) {
            gameClient.getHabbo().whisper("You are not on duty!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        RolePlay.getJobsManager().fireUser(gameClient.getHabbo(), data, habbo);
        data.executeAction();
        return true;
    }
}
