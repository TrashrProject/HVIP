package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

import java.text.SimpleDateFormat;
import java.util.List;

public class TransactionHistoryCommand extends Command {

    private static final int DEFAULT_HISTORY_LIMIT = 10;
    private static final SimpleDateFormat DATE_FORMAT = new SimpleDateFormat("dd/MM HH:mm");

    public TransactionHistoryCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (data.isPassive()) {
            gameClient.getHabbo().whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (!BankComputerSessionManager.mayUsePersonalBankCommand(gameClient.getHabbo())) {
            gameClient.getHabbo().whisper("En service Banque, connectez-vous à un ordinateur bancaire pour consulter l'historique.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        // Parse parameters - allow optional limit parameter
        int limit = DEFAULT_HISTORY_LIMIT;
        if (params.length == 2) {
            try {
                limit = Integer.parseInt(params[1]);
                if (limit <= 0 || limit > 20) {
                    gameClient.getHabbo().whisper("La limite doit \u00eatre comprise entre 1 et 20.", RoomChatMessageBubbles.ALERT);
                    return true;
                }
            } catch (NumberFormatException e) {
                    gameClient.getHabbo().whisper(":historique [limite] - Exemple : :historique 5", RoomChatMessageBubbles.ALERT);
                return true;
            }
        } else if (params.length > 2) {
            gameClient.getHabbo().whisper(":historique [limite] - Exemple : :historique 5", RoomChatMessageBubbles.ALERT);
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
            gameClient.getHabbo().whisper("Aucune transaction trouv\u00e9e.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        // Display header
        gameClient.getHabbo().whisper("=== Transactions r\u00e9centes ===", RoomChatMessageBubbles.ALERT);
        
        // Display transactions
        for (BankTransaction transaction : transactions) {
            String message = formatTransaction(transaction, userId);
            gameClient.getHabbo().whisper(message, RoomChatMessageBubbles.NORMAL);
        }
        
        // Display footer
        String footerMessage = String.format("=== %d transaction(s) affich\u00e9e(s) ===", transactions.size());
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
                message.append("D\u00e9p\u00f4t : +").append(amount);
                break;
            case WITHDRAW:
                String fee = RolePlay.getBankManager().formatCurrency(transaction.getFeeAmount());
                message.append("Retrait : -").append(amount).append(" (Frais : ").append(fee).append(")");
                break;
            case TRANSFER:
                if (transaction.getFromUserId() != null && transaction.getFromUserId().equals(currentUserId)) {
                    // Sent money
                    message.append("Envoy\u00e9 : -").append(amount).append(" (Virement sortant)");
                } else if (transaction.getToUserId() != null && transaction.getToUserId().equals(currentUserId)) {
                    // Received money
                    message.append("Re\u00e7u : +").append(amount).append(" (Virement entrant)");
                }
                break;
            case BANKER_DEPOSIT:
                message.append("Versement au guichet : +").append(amount);
                break;
            case BANKER_WITHDRAWAL:
                message.append("Retrait au guichet : -").append(amount);
                break;
            case ROBBERY:
                message.append("Braquage de distributeur : +").append(amount);
                break;
            case ATM_FEE:
                String feeAmount = RolePlay.getBankManager().formatCurrency(transaction.getFeeAmount());
                message.append("Frais de distributeur : -").append(feeAmount);
                break;
            default:
                message.append(type).append(": ").append(amount);
                break;
        }
        
        return message.toString();
    }
}
