package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.features.banking.entities.ATMRobbery;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;
import lombok.Getter;
import lombok.extern.slf4j.Slf4j;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

@Slf4j
public class BankManager {
    
    @Getter
    private final BankService bankService;

    public BankManager() {
        this.bankService = new BankService();
        log.info("BankManager initialized");
    }

    // Account Management
    public Optional<BankAccount> getBankAccount(int userId) {
        return bankService.getBankAccount(userId);
    }

    public boolean hasBankAccount(int userId) {
        return bankService.hasBankAccount(userId);
    }

    public BankAccount createBankAccount(int userId) {
        try {
            BankAccount account = bankService.createBankAccount(userId);
            this.pushBankSnapshot(userId); // send snapshot after account creation
            return account;
        } catch (Exception e) {
            log.error("Failed to create bank account for user {}", userId, e);
            throw e;
        }
    }

    public boolean closeBankAccount(int userId) { return bankService.closeBankAccount(userId); }
    public boolean bankerDeposit(int userId, BigDecimal amount, int roomId, int employeeId) {
        boolean ok = bankService.bankerDeposit(userId, amount, roomId, employeeId); if (ok) pushBankSnapshot(userId); return ok;
    }
    public boolean bankerWithdraw(int userId, BigDecimal amount, int roomId, int employeeId) {
        boolean ok = bankService.bankerWithdraw(userId, amount, roomId, employeeId); if (ok) pushBankSnapshot(userId); return ok;
    }

    // Banking Operations
    public boolean deposit(int userId, BigDecimal amount, int roomId) {
        if (!validateBankingOperation(userId, "deposit")) {
            return false;
        }
        boolean success = bankService.deposit(userId, amount, roomId);
        if (success) {
            this.pushBankSnapshot(userId);
        }
        return success;
    }
    public boolean mobileDeposit(int userId, BigDecimal amount, int roomId) {
        boolean success = bankService.mobileDeposit(userId, amount, roomId); if (success) pushBankSnapshot(userId); return success;
    }
    public long getMobileDepositCooldownSeconds(int userId) { return bankService.getMobileDepositCooldownSeconds(userId); }

    public boolean withdraw(int userId, BigDecimal amount, int roomId) {
        if (!validateBankingOperation(userId, "withdraw")) {
            return false;
        }
        boolean success = bankService.withdraw(userId, amount, roomId);
        if (success) {
            this.pushBankSnapshot(userId);
        }
        return success;
    }

    public boolean transfer(int fromUserId, int toUserId, BigDecimal amount, int roomId) {
        if (!validateBankingOperation(fromUserId, "transfer") ||
                !validateBankingOperation(toUserId, "transfer")) {
            return false;
        }
        boolean success = bankService.transfer(fromUserId, toUserId, amount, roomId);
        if (success) {
            this.pushBankSnapshot(fromUserId);
            this.pushBankSnapshot(toUserId);
        }
        return success;
    }

    // ATM Operations
    public boolean attemptATMRobbery(int userId, int roomId, int furniId, String weaponUsed) {
        log.info("ATM robbery attempt by user {} in room {} with weapon {}", userId, roomId, weaponUsed);
        boolean result = bankService.attemptATMRobbery(userId, roomId, furniId, weaponUsed);
        // Always push snapshot; wallet may change even on failed attempt (future logic)
        this.pushBankSnapshot(userId);
        return result;
    }

    // Validation Methods
    public boolean canDeposit(int userId, BigDecimal amount) {
        return bankService.canDeposit(userId, amount);
    }

    public boolean canWithdraw(int userId, BigDecimal amount) {
        return bankService.canWithdraw(userId, amount);
    }

    public boolean canTransfer(int fromUserId, int toUserId, BigDecimal amount) {
        return bankService.canTransfer(fromUserId, toUserId, amount);
    }

    // History and Information
    public List<BankTransaction> getTransactionHistory(int userId, int limit) {
        return bankService.getTransactionHistory(userId, Math.min(limit, 50)); // Limit to prevent abuse
    }

    public List<ATMRobbery> getATMRobberyHistory(int userId, int limit) {
        return bankService.getATMRobberyHistory(userId, Math.min(limit, 20)); // Limit to prevent abuse
    }

    // Utility Methods
    public String formatCurrency(BigDecimal amount) {
        return bankService.formatCurrency(amount);
    }

    public BigDecimal getATMFeePercentage() {
        return bankService.getATMFeePercentage();
    }

    public String getFormattedBalance(int userId) {
        Optional<BankAccount> accountOpt = getBankAccount(userId);
        if (accountOpt.isEmpty()) {
            return "Aucun compte bancaire";
        }
        
        // Get wallet balance from currency type 200
        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) {
            return "Joueur hors ligne";
        }
        
        BankAccount account = accountOpt.get();
        int walletBalance = habbo.getHabboInfo().getCredits();
        BigDecimal walletBalanceDecimal = BigDecimal.valueOf(walletBalance);
        BigDecimal totalBalance = account.getBankBalance().add(walletBalanceDecimal);
        
        return String.format("Banque : %s | Esp\u00e8ces : %s | Total : %s",
                           formatCurrency(account.getBankBalance()),
                           formatCurrency(walletBalanceDecimal),
                           formatCurrency(totalBalance));
    }

    // Error Messages for Commands
    public static final String ERROR_NO_BANK_ACCOUNT = "Vous n'avez pas de compte bancaire. Rendez-vous \u00e0 la banque pour en ouvrir un.";
    public static final String ERROR_INSUFFICIENT_WALLET = "Vous n'avez pas assez d'esp\u00e8ces pour effectuer ce d\u00e9p\u00f4t.";
    public static final String ERROR_INSUFFICIENT_BANK = "Votre compte ne contient pas assez d'argent pour ce retrait.";
    public static final String ERROR_INSUFFICIENT_FUNDS = "Vous n'avez pas assez d'argent pour effectuer ce virement.";
    public static final String ERROR_INVALID_AMOUNT = "Saisissez un montant valide.";
    public static final String ERROR_INVALID_USER = "Ce joueur est introuvable ou ne poss\u00e8de pas de compte bancaire.";
    public static final String ERROR_SAME_USER = "Vous ne pouvez pas vous envoyer de l'argent \u00e0 vous-m\u00eame.";
    public static final String ERROR_ROBBERY_NO_WEAPON = "Vous devez \u00e9quiper une batte pour braquer un distributeur.";
    public static final String ERROR_ROBBERY_FAILED = "Votre tentative de braquage a \u00e9chou\u00e9.";

    // Success Messages
    public static final String SUCCESS_ACCOUNT_CREATED = "Votre compte bancaire a \u00e9t\u00e9 ouvert.";
    public static final String SUCCESS_DEPOSIT = "Vous avez d\u00e9pos\u00e9 %s sur votre compte bancaire.";
    public static final String SUCCESS_WITHDRAW = "Vous avez retir\u00e9 %s de votre compte bancaire. Frais : %s.";
    public static final String SUCCESS_TRANSFER = "Vous avez envoy\u00e9 %s \u00e0 %s.";
    public static final String SUCCESS_ROBBERY = "Vous avez braqu\u00e9 le distributeur et obtenu %s.";

    // Balance check message format
    public static final String BALANCE_CHECK_FORMAT = "consulte son compte et constate un solde total de %s.";

    private boolean validateBankingOperation(int userId, String operation) {
        if (!hasBankAccount(userId)) {
            log.warn("User {} attempted {} without bank account", userId, operation);
            return false;
        }
        return true;
    }

    // Statistics methods for potential admin commands
    public long getTotalBankAccounts() {
        // This could be implemented if needed for statistics
        return 0; // Placeholder
    }

    public BigDecimal getTotalBankDeposits() {
        // This could be implemented if needed for statistics
        return BigDecimal.ZERO; // Placeholder
    }

    // ----------------------------------------------
    // NEW: Snapshot push logic
    // ----------------------------------------------
    private void pushBankSnapshot(int userId) {
        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) return;

        Optional<BankAccount> accountOpt = getBankAccount(userId);
        boolean hasAccount = accountOpt.isPresent();
        BankAccount account = accountOpt.orElse(null);
        BigDecimal bankBalance = hasAccount ? account.getBankBalance() : BigDecimal.ZERO;

        int walletInt = 0;
        try {
            walletInt = habbo.getHabboInfo().getCredits();
        } catch (Exception ignored) {}
        BigDecimal walletBalance = BigDecimal.valueOf(walletInt);
        BigDecimal feePercentage = getATMFeePercentage();

        boolean canDeposit = hasAccount && walletBalance.compareTo(BigDecimal.ZERO) > 0;
        boolean canWithdraw = hasAccount && bankBalance.compareTo(BigDecimal.ZERO) > 0;

        // Recent transactions (limit 20 for phone)
        List<BankTransaction> recentTransactions = hasAccount ? getTransactionHistory(userId, 20) : List.of();

        // Send ATM snapshot
        try {
            io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.AtmBankComposer atmComposer =
                    new io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.AtmBankComposer(
                            hasAccount,
                            hasAccount ? account.getAccountNumber() : "",
                            bankBalance,
                            walletBalance,
                            feePercentage,
                            canDeposit,
                            canWithdraw
                    );
            habbo.getClient().sendResponse(atmComposer);
        } catch (Exception e) {
            log.warn("Failed to send AtmBankComposer to user {}", userId, e);
        }

        // Send Phone snapshot + history
        try {
            io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.PhoneBankComposer phoneComposer =
                    new io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.PhoneBankComposer(
                            hasAccount,
                            hasAccount ? account.getAccountNumber() : "",
                            bankBalance,
                            walletBalance,
                            feePercentage,
                            canDeposit,
                            canWithdraw,
                            recentTransactions
                    );
            habbo.getClient().sendResponse(phoneComposer);
        } catch (Exception e) {
            log.warn("Failed to send PhoneBankComposer to user {}", userId, e);
        }
    }

    // Public wrapper for external callers (e.g., incoming request packet)
    public void sendBankSnapshot(int userId) {
        this.pushBankSnapshot(userId);
    }
}
