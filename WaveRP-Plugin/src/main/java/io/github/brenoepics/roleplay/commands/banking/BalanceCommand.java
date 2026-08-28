package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

import java.util.Optional;

public class BalanceCommand extends Command {

    public BalanceCommand(String permission, String[] keys) {
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
            gameClient.getHabbo().whisper(":solde", RoomChatMessageBubbles.ALERT);
            return true;
        }

        BankManager bankManager = RolePlay.getBankManager();
        int userId = gameClient.getHabbo().getHabboInfo().getId();
        
        if (!bankManager.hasBankAccount(userId)) {
            gameClient.getHabbo().whisper(BankManager.ERROR_NO_BANK_ACCOUNT, RoomChatMessageBubbles.ALERT);
            return true;
        }

        Optional<BankAccount> accountOpt = bankManager.getBankAccount(userId);
        if (accountOpt.isEmpty()) {
            gameClient.getHabbo().whisper("Impossible de r\u00e9cup\u00e9rer les informations du compte bancaire.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        BankAccount account = accountOpt.get();
        
        // Get wallet balance from currency type 200
        int walletBalance = gameClient.getHabbo().getHabboInfo().getCurrencyAmount(200);
        
        // Calculate total balance (bank + wallet)
        java.math.BigDecimal totalBalance = account.getBankBalance().add(java.math.BigDecimal.valueOf(walletBalance));
        String formattedBalance = bankManager.formatCurrency(totalBalance);
        String shoutMessage = String.format(BankManager.BALANCE_CHECK_FORMAT, formattedBalance);
        
        // Display balance as a shout bubble (public message)
        gameClient.getHabbo().shout(shoutMessage, RoomChatMessageBubbles.NORMAL);
        
        return true;
    }
}
