package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.BankComputerSessionManager;
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
            gameClient.getHabbo().whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (!BankComputerSessionManager.mayUsePersonalBankCommand(gameClient.getHabbo())) {
            gameClient.getHabbo().whisper("En service Banque, connectez-vous à un ordinateur bancaire pour effectuer un virement.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        
        if (params.length != 3) {
            gameClient.getHabbo().whisper(":virement <pseudo> <montant>", RoomChatMessageBubbles.ALERT);
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
            gameClient.getHabbo().whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT);
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
            String receiveMessage = String.format("Vous avez re\u00e7u %s de %s.",
                                                 formattedAmount, 
                                                 gameClient.getHabbo().getHabboInfo().getUsername());
            targetHabbo.whisper(receiveMessage, RoomChatMessageBubbles.ALERT);
            gameClient.getHabbo().shout("* Effectue un virement bancaire de "
                    + amount.toPlainString() + " crédits à " + targetHabbo.getHabboInfo().getUsername() + " *",
                    RoomChatMessageBubbles.NORMAL);
        } else {
            gameClient.getHabbo().whisper("Le virement a \u00e9chou\u00e9. R\u00e9essayez plus tard.", RoomChatMessageBubbles.ALERT);
        }
        
        return true;
    }
}
