package io.github.brenoepics.roleplay.commands.organizations;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class CreateOrganizationCommand extends Command {

    public CreateOrganizationCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length < 3) {
            gameClient.getHabbo().whisper(":create <gang/mafia/cartel> <name>", RoomChatMessageBubbles.ALERT);
            return true;
        }
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());

        if (data.getOrganizationId() != 0) {
            gameClient.getHabbo().whisper("You are already in an organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        OrganizationType type;
        try {
            type = OrganizationType.valueOf(params[1].toUpperCase());
            if (type == OrganizationType.ANY) {
                throw new IllegalArgumentException();
            }
        } catch (IllegalArgumentException e) {
            gameClient.getHabbo().whisper("Invalid organization type! must be <gang/mafia/cartel>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        StringBuilder name = new StringBuilder();

        for (int i = 2; i < params.length; i++) {
            name.append(params[i]).append(" ");
        }

        if (name.toString().length() > 15) {
            gameClient.getHabbo().whisper("The name of the organization cannot be longer than 15 characters!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if (RolePlay.getOrganizationManager().getOrganization(name.toString()) != null) {
            gameClient.getHabbo().whisper("There is already an organization with this name!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if(gameClient.getHabbo().getHabboInfo().getCurrencyAmount(200) < Emulator.getConfig().getInt("features.organizations."+ type.name().toLowerCase() +".price", 20)) {
            gameClient.getHabbo().whisper("You do not have enough bucks to create this organization!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean isCreated = RolePlay.getOrganizationManager().createOrganization(gameClient.getHabbo().getHabboInfo().getId(), name.toString(), type);
        if (isCreated) {
            gameClient.getHabbo().getHabboInfo().addCurrencyAmount(200, -Emulator.getConfig().getInt("features.organizations."+ type.name().toLowerCase() +".price", 20));
            gameClient.getHabbo().whisper("You have created the " + type.name().toLowerCase() + " " + name, RoomChatMessageBubbles.ALERT);
            return true;
        }

        gameClient.getHabbo().whisper("An error has occurred while creating the organization!", RoomChatMessageBubbles.ALERT);
        return true;
    }
}