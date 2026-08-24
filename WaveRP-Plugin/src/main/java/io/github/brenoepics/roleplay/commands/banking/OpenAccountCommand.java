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
            gameClient.getHabbo().whisper("You cannot execute RolePlay commands while passive mode is on!", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        if (params.length != 1) {
            gameClient.getHabbo().whisper(":openaccount", RoomChatMessageBubbles.ALERT);
            return true;
        }

        BankManager bankManager = RolePlay.getBankManager();
        int userId = gameClient.getHabbo().getHabboInfo().getId();
        
        // Check if user already has a bank account
        if (bankManager.hasBankAccount(userId)) {
            gameClient.getHabbo().whisper("You already have a bank account!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Check if user is in a bank room (optional - can be removed if not needed)
        int currentRoomId = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getId();
        if (!isBankRoom(currentRoomId)) {
            gameClient.getHabbo().whisper("You must be in a bank to open an account!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        try {
            // Create bank account
            BankAccount account = bankManager.createBankAccount(userId);
            
            String successMessage = String.format("%s Your account number is: %s", 
                                                 BankManager.SUCCESS_ACCOUNT_CREATED, 
                                                 account.getAccountNumber());
            gameClient.getHabbo().whisper(successMessage, RoomChatMessageBubbles.ALERT);
            
        } catch (Exception e) {
            gameClient.getHabbo().whisper("Failed to create bank account. Please try again later.", RoomChatMessageBubbles.ALERT);
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