package io.github.brenoepics.roleplay.features.banking.entities;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.sql.Timestamp;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class BankAccount {
    private int userId;
    private String accountNumber;
    private BigDecimal bankBalance;
    private Timestamp createdAt;
    private Timestamp updatedAt;

    public BankAccount(int userId, String accountNumber) {
        this.userId = userId;
        this.accountNumber = accountNumber;
        this.bankBalance = BigDecimal.ZERO;
        this.createdAt = new Timestamp(System.currentTimeMillis());
        this.updatedAt = new Timestamp(System.currentTimeMillis());
    }

    public boolean hasEnoughBankBalance(BigDecimal amount) {
        return bankBalance.compareTo(amount) >= 0;
    }

    public void depositToBank(BigDecimal amount) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            throw new IllegalArgumentException("Deposit amount must be positive");
        }
        
        bankBalance = bankBalance.add(amount);
        updatedAt = new Timestamp(System.currentTimeMillis());
    }

    public BigDecimal withdrawFromBank(BigDecimal amount, BigDecimal feePercentage) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            throw new IllegalArgumentException("Withdrawal amount must be positive");
        }
        if (!hasEnoughBankBalance(amount)) {
            throw new IllegalArgumentException("Insufficient bank balance");
        }

        // Calculate fee (e.g., 5% = 0.05)
        BigDecimal fee = amount.multiply(feePercentage);

        bankBalance = bankBalance.subtract(amount);
        updatedAt = new Timestamp(System.currentTimeMillis());

        return fee;
    }

    public void addToBankBalance(BigDecimal amount) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            throw new IllegalArgumentException("Amount must be positive");
        }
        bankBalance = bankBalance.add(amount);
        updatedAt = new Timestamp(System.currentTimeMillis());
    }

    public void subtractFromBankBalance(BigDecimal amount) {
        if (amount.compareTo(BigDecimal.ZERO) <= 0) {
            throw new IllegalArgumentException("Amount must be positive");
        }
        if (!hasEnoughBankBalance(amount)) {
            throw new IllegalArgumentException("Insufficient bank balance");
        }
        bankBalance = bankBalance.subtract(amount);
        updatedAt = new Timestamp(System.currentTimeMillis());
    }

    /**
     * Gets the wallet balance from Habbo's currency system (type 200)
     * @return wallet balance as BigDecimal
     */
    public BigDecimal getWalletBalance() {
        Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(userId);
        if (habbo == null) {
            return BigDecimal.ZERO;
        }
        int walletBalance = habbo.getHabboInfo().getCurrencyAmount(200);
        return BigDecimal.valueOf(walletBalance);
    }

    /**
     * Gets the total balance (bank balance + wallet balance)
     * @return total balance as BigDecimal
     */
    public BigDecimal getTotalBalance() {
        return bankBalance.add(getWalletBalance());
    }
}