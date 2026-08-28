package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboInfo;
import io.github.brenoepics.roleplay.features.banking.entities.ATMRobbery;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;
import lombok.extern.slf4j.Slf4j;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

@Slf4j
public class BankService {
    private static final BigDecimal ATM_FEE_PERCENTAGE = new BigDecimal("0.05"); // 5%
    private static final BigDecimal DEFAULT_STARTING_WALLET = new BigDecimal("100.00"); // Starting wallet amount
    
    private final BankRepository bankRepository;

    public BankService() {
        this.bankRepository = new BankRepository();
    }

    // Account Management
    public Optional<BankAccount> getBankAccount(int userId) {
        return bankRepository.findBankAccountByUserId(userId);
    }

    public boolean hasBankAccount(int userId) {
        return getBankAccount(userId).isPresent();
    }

    public BankAccount createBankAccount(int userId) {
        if (hasBankAccount(userId)) {
            throw new IllegalStateException("User already has a bank account");
        }

        String accountNumber = bankRepository.generateAccountNumber();
        if (accountNumber == null) {
            throw new RuntimeException("Failed to generate unique account number");
        }

        BankAccount account = new BankAccount(userId, accountNumber);
        
        if (bankRepository.createBankAccount(account)) {
            // Give starting wallet money through Habbo's currency system (type 200)
            Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
            if (habbo != null) {
                int startingAmount = DEFAULT_STARTING_WALLET.intValue();
                habbo.getHabboInfo().addCurrencyAmount(200, startingAmount);
            }
            
            log.info("Created bank account for user {} with account number {}", userId, accountNumber);
            return account;
        } else {
            throw new RuntimeException("Failed to create bank account in database");
        }
    }

    // Banking Operations
    public boolean deposit(int userId, BigDecimal amount, int roomId) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            return false;
        }

        Optional<BankAccount> accountOpt = getBankAccount(userId);
        if (accountOpt.isEmpty()) {
            return false;
        }

        // Check if user has enough currency type 200 (wallet)
        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) {
            return false;
        }
        
        int currentWallet = habbo.getHabboInfo().getCurrencyAmount(200);
        int depositAmount = amount.intValue();
        
        if (currentWallet < depositAmount) {
            return false; // Insufficient wallet balance
        }

        BankAccount account = accountOpt.get();
        try {
            // Deduct from currency type 200 and add to bank balance
            habbo.getHabboInfo().addCurrencyAmount(200, -depositAmount);
            account.depositToBank(amount);
            
            if (bankRepository.updateBankAccount(account)) {
                // Log transaction
                BankTransaction transaction = BankTransaction.createDeposit(userId, amount, roomId);
                bankRepository.saveTransaction(transaction);
                
                log.info("User {} deposited ${} to bank account", userId, amount);
                return true;
            }
        } catch (IllegalArgumentException e) {
            log.warn("Deposit failed for user {}: {}", userId, e.getMessage());
        }
        return false;
    }

    public boolean withdraw(int userId, BigDecimal amount, int roomId) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            return false;
        }

        Optional<BankAccount> accountOpt = getBankAccount(userId);
        if (accountOpt.isEmpty()) {
            return false;
        }

        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) {
            return false;
        }

        BankAccount account = accountOpt.get();
        try {
            // Calculate fee (5%) and amount after fee
            BigDecimal fee = amount.multiply(ATM_FEE_PERCENTAGE);
            BigDecimal amountAfterFee = amount.subtract(fee);
            
            // Deduct from bank balance and add to currency type 200 (wallet)
            account.withdrawFromBank(amount, ATM_FEE_PERCENTAGE);
            habbo.getHabboInfo().addCurrencyAmount(200, amountAfterFee.intValue());
            
            if (bankRepository.updateBankAccount(account)) {
                // Log withdrawal transaction
                BankTransaction transaction = BankTransaction.createWithdrawal(userId, amount, fee, roomId);
                bankRepository.saveTransaction(transaction);
                
                // Log fee transaction if there was a fee
                if (fee.compareTo(BigDecimal.ZERO) > 0) {
                    BankTransaction feeTransaction = BankTransaction.createAtmFee(userId, fee, roomId);
                    bankRepository.saveTransaction(feeTransaction);
                }
                
                log.info("User {} withdrew ${} from bank account (fee: ${})", userId, amount, fee);
                return true;
            }
        } catch (IllegalArgumentException e) {
            log.warn("Withdrawal failed for user {}: {}", userId, e.getMessage());
        }
        return false;
    }

    public boolean transfer(int fromUserId, int toUserId, BigDecimal amount, int roomId) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            return false;
        }

        if (fromUserId == toUserId) {
            return false;
        }

        Optional<BankAccount> fromAccountOpt = getBankAccount(fromUserId);
        Optional<BankAccount> toAccountOpt = getBankAccount(toUserId);
        
        if (fromAccountOpt.isEmpty() || toAccountOpt.isEmpty()) {
            return false;
        }

        Habbo fromHabbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(fromUserId);
        Habbo toHabbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(toUserId);
        
        if (fromHabbo == null || toHabbo == null) {
            return false;
        }

        BankAccount fromAccount = fromAccountOpt.get();
        BankAccount toAccount = toAccountOpt.get();

        // Get wallet balances from currency type 200
        int fromWallet = fromHabbo.getHabboInfo().getCurrencyAmount(200);
        BigDecimal fromBankBalance = fromAccount.getBankBalance();
        BigDecimal totalAvailable = fromBankBalance.add(BigDecimal.valueOf(fromWallet));
        
        if (totalAvailable.compareTo(amount) < 0) {
            return false; // Insufficient funds
        }

        int transferAmount = amount.intValue();
        
        try {
            if (fromWallet >= transferAmount) {
                // Can pay entirely from wallet
                fromHabbo.getHabboInfo().addCurrencyAmount(200, -transferAmount);
                toHabbo.getHabboInfo().addCurrencyAmount(200, transferAmount);
            } else {
                // Need to use both wallet and bank balance
                int bankAmount = transferAmount - fromWallet;
                
                // Deduct all wallet money first
                if (fromWallet > 0) {
                    fromHabbo.getHabboInfo().addCurrencyAmount(200, -fromWallet);
                }
                // Deduct remaining from bank balance
                fromAccount.subtractFromBankBalance(BigDecimal.valueOf(bankAmount));
                // Give all to recipient's wallet
                toHabbo.getHabboInfo().addCurrencyAmount(200, transferAmount);
            }

            // Update accounts in database
            if (bankRepository.updateBankAccount(fromAccount)) {
                // Log transaction
                BankTransaction transaction = BankTransaction.createTransfer(fromUserId, toUserId, amount, roomId);
                bankRepository.saveTransaction(transaction);
                
                log.info("User {} transferred ${} to user {}", fromUserId, amount, toUserId);
                return true;
            }
        } catch (IllegalArgumentException e) {
            log.warn("Transfer failed for users {} -> {}: {}", fromUserId, toUserId, e.getMessage());
        }

        return false;
    }

    // ATM Robbery Operations
    public boolean attemptATMRobbery(int userId, int roomId, int furniId, String weaponUsed) {
        // Basic robbery logic - this can be enhanced with more complex mechanics
        boolean hasCorrectWeapon = "Bat".equalsIgnoreCase(weaponUsed);
        
        if (!hasCorrectWeapon) {
            // Failed robbery - no bat equipped
            ATMRobbery failedRobbery = ATMRobbery.createFailedRobbery(userId, roomId, furniId, weaponUsed, false);
            bankRepository.saveATMRobbery(failedRobbery);
            return false;
        }

        // Calculate success rate (can be enhanced with more factors)
        boolean success = Math.random() < 0.7; // 70% success rate with bat
        boolean policeAlerted = Math.random() < 0.3; // 30% chance police are alerted
        
        ATMRobbery robbery;
        if (success) {
            // Generate random amount stolen (100-1000)
            BigDecimal amountStolen = BigDecimal.valueOf(100 + (Math.random() * 900));
            amountStolen = amountStolen.setScale(2, BigDecimal.ROUND_HALF_UP);
            
            robbery = ATMRobbery.createSuccessfulRobbery(userId, roomId, furniId, amountStolen, weaponUsed, policeAlerted);
            
            // Add stolen money to user's wallet (currency type 200)
            Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
            if (habbo != null) {
                habbo.getHabboInfo().addCurrencyAmount(200, amountStolen.intValue());
                
                // Log robbery transaction
                BankTransaction transaction = BankTransaction.createAtmRobbery(userId, amountStolen, roomId);
                bankRepository.saveTransaction(transaction);
            }
            
            log.info("User {} successfully robbed ATM for ${}", userId, amountStolen);
        } else {
            robbery = ATMRobbery.createFailedRobbery(userId, roomId, furniId, weaponUsed, policeAlerted);
            log.info("User {} failed to rob ATM", userId);
        }

        bankRepository.saveATMRobbery(robbery);
        return success;
    }

    // Utility Methods
    public List<BankTransaction> getTransactionHistory(int userId, int limit) {
        return bankRepository.getTransactionsByUserId(userId, limit);
    }

    public List<ATMRobbery> getATMRobberyHistory(int userId, int limit) {
        return bankRepository.getATMRobberiesByUserId(userId, limit);
    }

    public BigDecimal getATMFeePercentage() {
        return ATM_FEE_PERCENTAGE;
    }

    public String formatCurrency(BigDecimal amount) {
        return String.format("$%.2f", amount);
    }

    // Validation Methods
    public boolean canDeposit(int userId, BigDecimal amount) {
        Optional<BankAccount> accountOpt = getBankAccount(userId);
        if (accountOpt.isEmpty() || amount.compareTo(BigDecimal.ZERO) <= 0) {
            return false;
        }
        
        // Check currency type 200 (wallet balance)
        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) {
            return false;
        }
        
        int walletBalance = habbo.getHabboInfo().getCurrencyAmount(200);
        return walletBalance >= amount.intValue();
    }

    public boolean canWithdraw(int userId, BigDecimal amount) {
        Optional<BankAccount> accountOpt = getBankAccount(userId);
        return accountOpt.isPresent() && 
               amount.compareTo(BigDecimal.ZERO) > 0 && 
               accountOpt.get().hasEnoughBankBalance(amount);
    }

    public boolean canTransfer(int fromUserId, int toUserId, BigDecimal amount) {
        Optional<BankAccount> fromAccountOpt = getBankAccount(fromUserId);
        Optional<BankAccount> toAccountOpt = getBankAccount(toUserId);
        
        if (fromUserId == toUserId || fromAccountOpt.isEmpty() || toAccountOpt.isEmpty() || 
            amount.compareTo(BigDecimal.ZERO) <= 0) {
            return false;
        }
        
        // Calculate total available funds (wallet + bank balance)
        Habbo fromHabbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(fromUserId);
        if (fromHabbo == null) {
            return false;
        }
        
        int walletBalance = fromHabbo.getHabboInfo().getCurrencyAmount(200);
        BigDecimal bankBalance = fromAccountOpt.get().getBankBalance();
        BigDecimal totalAvailable = bankBalance.add(BigDecimal.valueOf(walletBalance));
        
        return totalAvailable.compareTo(amount) >= 0;
    }
}