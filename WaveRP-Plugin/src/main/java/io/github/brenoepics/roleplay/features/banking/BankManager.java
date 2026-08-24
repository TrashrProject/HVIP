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
            return "No bank account";
        }
        
        // Get wallet balance from currency type 200
        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) {
            return "User not online";
        }
        
        BankAccount account = accountOpt.get();
        int walletBalance = habbo.getHabboInfo().getCurrencyAmount(200);
        BigDecimal walletBalanceDecimal = BigDecimal.valueOf(walletBalance);
        BigDecimal totalBalance = account.getBankBalance().add(walletBalanceDecimal);
        
        return String.format("Bank: %s | Wallet: %s | Total: %s", 
                           formatCurrency(account.getBankBalance()),
                           formatCurrency(walletBalanceDecimal),
                           formatCurrency(totalBalance));
    }

    // Error Messages for Commands
    public static final String ERROR_NO_BANK_ACCOUNT = "Sorry, you don't have a bank account, visit the bank to open one!";
    public static final String ERROR_INSUFFICIENT_WALLET = "You don't have this amount to deposit!";
    public static final String ERROR_INSUFFICIENT_BANK = "You don't have this amount to withdraw!";
    public static final String ERROR_INSUFFICIENT_FUNDS = "You don't have enough money for this transfer!";
    public static final String ERROR_INVALID_AMOUNT = "Please enter a valid amount!";
    public static final String ERROR_INVALID_USER = "User not found or doesn't have a bank account!";
    public static final String ERROR_SAME_USER = "You cannot transfer money to yourself!";
    public static final String ERROR_ROBBERY_NO_WEAPON = "You need to have a bat equipped to rob an ATM!";
    public static final String ERROR_ROBBERY_FAILED = "Your robbery attempt failed!";

    // Success Messages
    public static final String SUCCESS_ACCOUNT_CREATED = "Bank account created successfully! Welcome to the banking system!";
    public static final String SUCCESS_DEPOSIT = "Successfully deposited %s to your bank account!";
    public static final String SUCCESS_WITHDRAW = "Successfully withdrew %s from your bank account! (Fee: %s)";
    public static final String SUCCESS_TRANSFER = "Successfully transferred %s to %s!";
    public static final String SUCCESS_ROBBERY = "You successfully robbed the ATM and got %s!";

    // Balance check message format
    public static final String BALANCE_CHECK_FORMAT = "Checks their account balance and notices they have %s.";

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
            walletInt = habbo.getHabboInfo().getCurrencyAmount(200);
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