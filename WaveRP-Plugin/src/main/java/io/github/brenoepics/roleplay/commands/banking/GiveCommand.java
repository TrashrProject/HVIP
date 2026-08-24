package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

import java.math.BigDecimal;

public class GiveCommand extends Command {

    public GiveCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (data.isPassive()) {
            gameClient.getHabbo().whisper("You cannot execute RolePlay commands while passive mode is on!", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        if (params.length != 3) {
            gameClient.getHabbo().whisper(":give <username> <amount>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        String targetUsername = params[1];
        String amountStr = params[2];
        
        // Parse amount
        BigDecimal amount;
        try {
            amount = new BigDecimal(amountStr);
            if (amount.compareTo(BigDecimal.ZERO) <= 0) {
                gameClient.getHabbo().whisper(BankManager.ERROR_INVALID_AMOUNT, RoomChatMessageBubbles.ALERT);
                return true;
            }
        } catch (NumberFormatException e) {
            gameClient.getHabbo().whisper(BankManager.ERROR_INVALID_AMOUNT, RoomChatMessageBubbles.ALERT);
            return true;
        }

        BankManager bankManager = RolePlay.getBankManager();
        int fromUserId = gameClient.getHabbo().getHabboInfo().getId();
        
        // Check if sender has bank account
        if (!bankManager.hasBankAccount(fromUserId)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_NO_BANK_ACCOUNT, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Find target user (must be in same room for security)
        Habbo targetHabbo = gameClient.getHabbo().getHabboInfo().getCurrentRoom().getHabbo(targetUsername);
        if (targetHabbo == null) {
            gameClient.getHabbo().whisper("User not found in current room!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        int toUserId = targetHabbo.getHabboInfo().getId();
        
        // Check if trying to send to self
        if (fromUserId == toUserId) {
            gameClient.getHabbo().whisper(BankManager.ERROR_SAME_USER, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Check if target has bank account
        if (!bankManager.hasBankAccount(toUserId)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_INVALID_USER, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Validate transfer
        if (!bankManager.canTransfer(fromUserId, toUserId, amount)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_INSUFFICIENT_FUNDS, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Perform transfer
        int roomId = gameClient.getHabbo().getHabboInfo().getCurrentRoom() != null ? 
                    gameClient.getHabbo().getHabboInfo().getCurrentRoom().getId() : -1;
        
        if (bankManager.transfer(fromUserId, toUserId, amount, roomId)) {
            String formattedAmount = bankManager.formatCurrency(amount);
            String successMessage = String.format(BankManager.SUCCESS_TRANSFER, formattedAmount, targetUsername);
            gameClient.getHabbo().whisper(successMessage, RoomChatMessageBubbles.ALERT);
            
            // Notify target user (already in same room so already online)
            String receiveMessage = String.format("You received %s from %s!", 
                                                 formattedAmount, 
                                                 gameClient.getHabbo().getHabboInfo().getUsername());
            targetHabbo.whisper(receiveMessage, RoomChatMessageBubbles.ALERT);
        } else {
            gameClient.getHabbo().whisper("Transfer failed. Please try again later.", RoomChatMessageBubbles.ALERT);
        }
        
        return true;
    }
}