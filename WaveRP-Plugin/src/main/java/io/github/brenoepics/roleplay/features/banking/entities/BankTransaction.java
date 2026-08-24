package io.github.brenoepics.roleplay.features.banking.entities;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.sql.Timestamp;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class BankTransaction {
    public enum TransactionType {
        DEPOSIT, WITHDRAW, TRANSFER, ATM_FEE, ROBBERY
    }

    private int id;
    private Integer fromUserId;
    private Integer toUserId;
    private TransactionType transactionType;
    private BigDecimal amount;
    private BigDecimal feeAmount;
    private String description;
    private Integer roomId;
    private Timestamp createdAt;

    public BankTransaction(Integer fromUserId, Integer toUserId, TransactionType transactionType, 
                          BigDecimal amount, BigDecimal feeAmount, String description, Integer roomId) {
        this.fromUserId = fromUserId;
        this.toUserId = toUserId;
        this.transactionType = transactionType;
        this.amount = amount;
        this.feeAmount = feeAmount != null ? feeAmount : BigDecimal.ZERO;
        this.description = description;
        this.roomId = roomId;
        this.createdAt = new Timestamp(System.currentTimeMillis());
    }

    public static BankTransaction createDeposit(int userId, BigDecimal amount, int roomId) {
        return new BankTransaction(userId, userId, TransactionType.DEPOSIT, amount, BigDecimal.ZERO, 
                                 "Deposit to bank account", roomId);
    }

    public static BankTransaction createWithdrawal(int userId, BigDecimal amount, BigDecimal fee, int roomId) {
        return new BankTransaction(userId, userId, TransactionType.WITHDRAW, amount, fee, 
                                 "Withdrawal from bank account", roomId);
    }

    public static BankTransaction createTransfer(int fromUserId, int toUserId, BigDecimal amount, int roomId) {
        return new BankTransaction(fromUserId, toUserId, TransactionType.TRANSFER, amount, BigDecimal.ZERO, 
                                 "Money transfer", roomId);
    }

    public static BankTransaction createAtmRobbery(int userId, BigDecimal amount, int roomId) {
        return new BankTransaction(null, userId, TransactionType.ROBBERY, amount, BigDecimal.ZERO, 
                                 "ATM robbery", roomId);
    }

    public static BankTransaction createAtmFee(int userId, BigDecimal feeAmount, int roomId) {
        return new BankTransaction(userId, null, TransactionType.ATM_FEE, BigDecimal.ZERO, feeAmount, 
                                 "ATM withdrawal fee", roomId);
    }

    public boolean isDeposit() {
        return transactionType == TransactionType.DEPOSIT;
    }

    public boolean isWithdrawal() {
        return transactionType == TransactionType.WITHDRAW;
    }

    public boolean isTransfer() {
        return transactionType == TransactionType.TRANSFER;
    }

    public boolean isRobbery() {
        return transactionType == TransactionType.ROBBERY;
    }

    public boolean isAtmFee() {
        return transactionType == TransactionType.ATM_FEE;
    }
}