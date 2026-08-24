package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.ROB_TIMEOUT;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;

public class RobCommand extends Command {
    public RobCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar attackerData = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (attackerData.isPassive()) {
            gameClient.getHabbo().whisper("You cannot execute RolePlay commands while passive mode is on!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length != 3) {
            gameClient.getHabbo().whisper(":rob <player> <amount>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!attackerData.hasEnergy()) {
            gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            gameClient.getHabbo().whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (habbo == gameClient.getHabbo()) {
            gameClient.getHabbo().whisper("You cannot rob yourself!", RoomChatMessageBubbles.ALERT);
            return true;
        }
        int bucks;
        try {
            bucks = Integer.parseInt(params[2]);
        } catch (NumberFormatException e) {
            gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.error.cmd_credits.invalid_amount"), RoomChatMessageBubbles.ALERT);
            return true;
        }

        RoomTile tFront = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getLayout().getTileInFront(gameClient.getHabbo().getRoomUnit().getCurrentLocation(), gameClient.getHabbo().getRoomUnit().getBodyRotation().getValue());
        if (tFront == null) return true;

        RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(habbo);
        if (targetData.isPassive() && !targetData.isAggressive()) {
            gameClient.getHabbo().whisper("You can't hit " + habbo.getHabboInfo().getUsername()
                + " because they are in passive mode.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (attackerData.isAtSafeZone() && (!targetData.isAggressive() || !attackerData.isAggressive())) {
            gameClient.getHabbo().whisper("Sorry you can not use this command while in a safe zone.",
                RoomChatMessageBubbles.ALERT);
            return true;
        }

        if(!Emulator.getConfig().getBoolean("features.organizations.friendly_fire") && attackerData.getOrganizationId() == targetData.getOrganizationId()) {
            gameClient.getHabbo().whisper("You can't rob " + habbo.getHabboInfo().getUsername() + " because they are in the same organization as you.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (tFront.x != habbo.getRoomUnit().getX() || tFront.y != habbo.getRoomUnit().getY()) {
            gameClient.getHabbo().whisper("User " + params[1] + " is too far away", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (habbo.getHabboInfo().getCurrencyAmount(200) < bucks) {
            gameClient.getHabbo().whisper("User " + params[1] + " does not have so much money", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("rob").getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
        if (timeout != null) {
            gameClient.getHabbo().whisper("You have to wait " + timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " seconds to use this command again!");
            return true;
        }

        gameClient.getHabbo().whisper("You have stolen " + params[1] + " " + bucks + " bucks", RoomChatMessageBubbles.ALERT);
        habbo.whisper("You have been robbed of " + bucks + " bucks");
        habbo.givePoints(200, -bucks);
        gameClient.getHabbo().givePoints(200, bucks);
        attackerData.executeAction();
        RolePlay.getCommandsCounter().getCoolDown("rob").addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), ROB_TIMEOUT);
        return true;
    }

}
