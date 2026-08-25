package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class OpenAccountCommand extends Command {

    public OpenAccountCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (data.isPassive()) {
            gameClient.getHabbo().whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        if (params.length != 1) {
            gameClient.getHabbo().whisper(":ouvrircompte", RoomChatMessageBubbles.ALERT);
            return true;
        }

        BankManager bankManager = RolePlay.getBankManager();
        int userId = gameClient.getHabbo().getHabboInfo().getId();
        
        // Check if user already has a bank account
        if (bankManager.hasBankAccount(userId)) {
            gameClient.getHabbo().whisper("Vous poss\u00e9dez d\u00e9j\u00e0 un compte bancaire.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Check if user is in a bank room (optional - can be removed if not needed)
        int currentRoomId = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getId();
        if (!isBankRoom(currentRoomId)) {
            gameClient.getHabbo().whisper("Vous devez \u00eatre dans une banque pour ouvrir un compte.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        try {
            // Create bank account
            BankAccount account = bankManager.createBankAccount(userId);
            
            String successMessage = String.format("%s Votre num\u00e9ro de compte est : %s",
                                                 BankManager.SUCCESS_ACCOUNT_CREATED, 
                                                 account.getAccountNumber());
            gameClient.getHabbo().whisper(successMessage, RoomChatMessageBubbles.ALERT);
            
        } catch (Exception e) {
            gameClient.getHabbo().whisper("L'ouverture du compte a \u00e9chou\u00e9. R\u00e9essayez plus tard.", RoomChatMessageBubbles.ALERT);
        }
        
        return true;
    }

    /**
     * Checks if the current room is a bank room.
     * This method can be enhanced to check against a list of bank room IDs
     * or use the jobs_rooms table to determine bank locations.
     */
    private boolean isBankRoom(int roomId) {
        // For now, we'll allow account opening in any room
        // This can be enhanced later to check specific bank rooms
        // Example implementation:
        // - Check against jobs_rooms table where job = 'bank'
        // - Or maintain a list of bank room IDs
        return true;
    }
}
