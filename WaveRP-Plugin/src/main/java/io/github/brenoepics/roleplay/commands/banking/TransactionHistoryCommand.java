package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

import java.text.SimpleDateFormat;
import java.util.List;

public class TransactionHistoryCommand extends Command {

    private static final int DEFAULT_HISTORY_LIMIT = 10;
    private static final SimpleDateFormat DATE_FORMAT = new SimpleDateFormat("MM/dd HH:mm");

    public TransactionHistoryCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (data.isPassive()) {
            gameClient.getHabbo().whisper("You cannot execute RolePlay commands while passive mode is on!", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        // Parse parameters - allow optional limit parameter
        int limit = DEFAULT_HISTORY_LIMIT;
        if (params.length == 2) {
            try {
                limit = Integer.parseInt(params[1]);
                if (limit <= 0 || limit > 20) {
                    gameClient.getHabbo().whisper("Limit must be between 1 and 20!", RoomChatMessageBubbles.ALERT);
                    return true;
                }
            } catch (NumberFormatException e) {
                gameClient.getHabbo().whisper(":transactions [limit] - Example: :transactions 5", RoomChatMessageBubbles.ALERT);
                return true;
            }
        } else if (params.length > 2) {
            gameClient.getHabbo().whisper(":transactions [limit] - Example: :transactions 5", RoomChatMessageBubbles.ALERT);
            return true;
        }

        BankManager bankManager = RolePlay.getBankManager();
        int userId = gameClient.getHabbo().getHabboInfo().getId();
        
        // Check if user has bank account
        if (!bankManager.hasBankAccount(userId)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_NO_BANK_ACCOUNT, RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Get transaction history
        List<BankTransaction> transactions = bankManager.getTransactionHistory(userId, limit);
        
        if (transactions.isEmpty()) {
            gameClient.getHabbo().whisper("No transaction history found.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Display header
        gameClient.getHabbo().whisper("=== Recent Transactions ===", RoomChatMessageBubbles.ALERT);
        
        // Display transactions
        for (BankTransaction transaction : transactions) {
            String message = formatTransaction(transaction, userId);
            gameClient.getHabbo().whisper(message, RoomChatMessageBubbles.NORMAL);
        }
        
        // Display footer
        String footerMessage = String.format("=== Showing %d of your recent transactions ===", transactions.size());
        gameClient.getHabbo().whisper(footerMessage, RoomChatMessageBubbles.ALERT);
        
        return true;
    }

    private String formatTransaction(BankTransaction transaction, int currentUserId) {
        String date = DATE_FORMAT.format(transaction.getCreatedAt());
        String amount = RolePlay.getBankManager().formatCurrency(transaction.getAmount());
        String type = transaction.getTransactionType().name().toLowerCase();
        
        StringBuilder message = new StringBuilder();
        message.append("[").append(date).append("] ");
        
        switch (transaction.getTransactionType()) {
            case DEPOSIT:
                message.append("Deposit: +").append(amount);
                break;
            case WITHDRAW:
                String fee = RolePlay.getBankManager().formatCurrency(transaction.getFeeAmount());
                message.append("Withdraw: -").append(amount).append(" (Fee: ").append(fee).append(")");
                break;
            case TRANSFER:
                if (transaction.getFromUserId() != null && transaction.getFromUserId().equals(currentUserId)) {
                    // Sent money
                    message.append("Sent: -").append(amount).append(" (Transfer out)");
                } else if (transaction.getToUserId() != null && transaction.getToUserId().equals(currentUserId)) {
                    // Received money
                    message.append("Received: +").append(amount).append(" (Transfer in)");
                }
                break;
            case ROBBERY:
                message.append("ATM Robbery: +").append(amount);
                break;
            case ATM_FEE:
                String feeAmount = RolePlay.getBankManager().formatCurrency(transaction.getFeeAmount());
                message.append("ATM Fee: -").append(feeAmount);
                break;
            default:
                message.append(type).append(": ").append(amount);
                break;
        }
        
        return message.toString();
    }
}