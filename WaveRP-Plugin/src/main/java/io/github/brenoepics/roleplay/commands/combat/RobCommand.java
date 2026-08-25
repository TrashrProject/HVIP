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
            gameClient.getHabbo().whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (params.length != 3) {
            gameClient.getHabbo().whisper(":braquer <pseudo> <montant>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (!attackerData.hasEnergy()) {
            gameClient.getHabbo().whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
            return true;
        }

        Habbo habbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(params[1]);
        if (habbo == null) {
            gameClient.getHabbo().whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (habbo == gameClient.getHabbo()) {
            gameClient.getHabbo().whisper("Vous ne pouvez pas vous braquer vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
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
            gameClient.getHabbo().whisper("Vous ne pouvez pas braquer " + habbo.getHabboInfo().getUsername()
                + " car ce joueur est en mode passif.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (attackerData.isAtSafeZone() && (!targetData.isAggressive() || !attackerData.isAggressive())) {
            gameClient.getHabbo().whisper("Vous ne pouvez pas utiliser cette commande dans une zone prot\u00e9g\u00e9e.",
                RoomChatMessageBubbles.ALERT);
            return true;
        }

        if(!Emulator.getConfig().getBoolean("features.organizations.friendly_fire") && attackerData.getOrganizationId() == targetData.getOrganizationId()) {
            gameClient.getHabbo().whisper("Vous ne pouvez pas braquer " + habbo.getHabboInfo().getUsername() + " car vous appartenez \u00e0 la m\u00eame organisation.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (tFront.x != habbo.getRoomUnit().getX() || tFront.y != habbo.getRoomUnit().getY()) {
            gameClient.getHabbo().whisper("Le joueur " + params[1] + " est trop loin.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (habbo.getHabboInfo().getCurrencyAmount(200) < bucks) {
            gameClient.getHabbo().whisper("Le joueur " + params[1] + " ne poss\u00e8de pas ce montant.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("rob").getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
        if (timeout != null) {
            gameClient.getHabbo().whisper("Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " seconde(s) avant de r\u00e9utiliser cette commande.");
            return true;
        }

        gameClient.getHabbo().whisper("Vous avez vol\u00e9 " + bucks + " $ \u00e0 " + params[1] + ".", RoomChatMessageBubbles.ALERT);
        habbo.whisper("On vous a vol\u00e9 " + bucks + " $.");
        habbo.givePoints(200, -bucks);
        gameClient.getHabbo().givePoints(200, bucks);
        attackerData.executeAction();
        RolePlay.getCommandsCounter().getCoolDown("rob").addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), ROB_TIMEOUT);
        return true;
    }

}
