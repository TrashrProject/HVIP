package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.HabboItem;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionATM;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

import java.math.BigDecimal;

public class WithdrawCommand extends Command {

    public WithdrawCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (data.isPassive()) {
            gameClient.getHabbo().whisper("You cannot execute RolePlay commands while passive mode is on!", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        if (params.length != 2) {
            gameClient.getHabbo().whisper(":withdraw <amount>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        String amountStr = params[1];
        
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
        int userId = gameClient.getHabbo().getHabboInfo().getId();
        
        // Check if user has bank account
        if (!bankManager.hasBankAccount(userId)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_NO_BANK_ACCOUNT, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Check if user is near an ATM
        Room currentRoom = gameClient.getHabbo().getHabboInfo().getCurrentRoom();
        if (currentRoom == null) {
            gameClient.getHabbo().whisper("You must be in a room to use banking commands!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean nearATM = isNearATM(currentRoom, gameClient.getHabbo());
        if (!nearATM) {
            gameClient.getHabbo().whisper("You must be near an ATM to make withdrawals!", RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Validate withdrawal
        if (!bankManager.canWithdraw(userId, amount)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_INSUFFICIENT_BANK, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Show fee information before withdrawal
        BigDecimal feePercentage = bankManager.getATMFeePercentage();
        BigDecimal fee = amount.multiply(feePercentage);
        BigDecimal amountAfterFee = amount.subtract(fee);
        
        String feeInfo = String.format("ATM Fee: %s (%.0f%%) | You will receive: %s", 
                                      bankManager.formatCurrency(fee), 
                                      feePercentage.multiply(BigDecimal.valueOf(100)), 
                                      bankManager.formatCurrency(amountAfterFee));
        gameClient.getHabbo().whisper(feeInfo, RoomChatMessageBubbles.NORMAL);

        // Perform withdrawal
        int roomId = currentRoom.getId();
        if (bankManager.withdraw(userId, amount, roomId)) {
            String formattedAmount = bankManager.formatCurrency(amount);
            String formattedFee = bankManager.formatCurrency(fee);
            String successMessage = String.format(BankManager.SUCCESS_WITHDRAW, formattedAmount, formattedFee);
            gameClient.getHabbo().whisper(successMessage, RoomChatMessageBubbles.ALERT);
        } else {
            gameClient.getHabbo().whisper("Withdrawal failed. Please try again later.", RoomChatMessageBubbles.ALERT);
        }
        
        return true;
    }

    /**
     * Check if user is near an ATM in the current room
     */
    private boolean isNearATM(Room room, com.eu.habbo.habbohotel.users.Habbo habbo) {
        // Check all furniture items in the room for ATMs
        for (HabboItem item : room.getFloorItems()) {
            // Check if this item is an ATM (you might need to check by base item ID or interaction type)
            if (isATMItem(item)) {
                if (InteractionATM.isUserAroundATM(habbo, room, item)) {
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Check if a furniture item is an ATM
     * This can be enhanced to check against specific ATM furniture IDs
     */
    private boolean isATMItem(HabboItem item) {
        // For now, check if the item has "atm" in its name (case insensitive)
        // This can be enhanced to check against specific base item IDs from the database
        String itemName = item.getBaseItem().getName().toLowerCase();
        return itemName.contains("atm") || itemName.contains("bank");
    }
}