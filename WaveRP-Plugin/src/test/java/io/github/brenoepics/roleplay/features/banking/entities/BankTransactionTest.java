package io.github.brenoepics.roleplay.features.banking.entities;

import org.junit.jupiter.api.Test;

import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.*;

class BankTransactionTest {

    private static final int USER_ID_1 = 123;
    private static final int USER_ID_2 = 456;
    private static final int ROOM_ID = 789;
    private static final BigDecimal AMOUNT = new BigDecimal("100.00");
    private static final BigDecimal FEE = new BigDecimal("5.00");

    @Test
    void testCreateDeposit() {
        BankTransaction transaction = BankTransaction.createDeposit(USER_ID_1, AMOUNT, ROOM_ID);

        assertEquals(USER_ID_1, transaction.getFromUserId());
        assertEquals(USER_ID_1, transaction.getToUserId());
        assertEquals(BankTransaction.TransactionType.DEPOSIT, transaction.getTransactionType());
        assertEquals(AMOUNT, transaction.getAmount());
        assertEquals(BigDecimal.ZERO, transaction.getFeeAmount());
        assertEquals("Deposit to bank account", transaction.getDescription());
        assertEquals(ROOM_ID, transaction.getRoomId());
        assertNotNull(transaction.getCreatedAt());
        assertTrue(transaction.isDeposit());
    }

    @Test
    void testCreateWithdrawal() {
        BankTransaction transaction = BankTransaction.createWithdrawal(USER_ID_1, AMOUNT, FEE, ROOM_ID);

        assertEquals(USER_ID_1, transaction.getFromUserId());
        assertEquals(USER_ID_1, transaction.getToUserId());
        assertEquals(BankTransaction.TransactionType.WITHDRAW, transaction.getTransactionType());
        assertEquals(AMOUNT, transaction.getAmount());
        assertEquals(FEE, transaction.getFeeAmount());
        assertEquals("Withdrawal from bank account", transaction.getDescription());
        assertEquals(ROOM_ID, transaction.getRoomId());
        assertNotNull(transaction.getCreatedAt());
        assertTrue(transaction.isWithdrawal());
    }

    @Test
    void testCreateTransfer() {
        BankTransaction transaction = BankTransaction.createTransfer(USER_ID_1, USER_ID_2, AMOUNT, ROOM_ID);

        assertEquals(USER_ID_1, transaction.getFromUserId());
        assertEquals(USER_ID_2, transaction.getToUserId());
        assertEquals(BankTransaction.TransactionType.TRANSFER, transaction.getTransactionType());
        assertEquals(AMOUNT, transaction.getAmount());
        assertEquals(BigDecimal.ZERO, transaction.getFeeAmount());
        assertEquals("Money transfer", transaction.getDescription());
        assertEquals(ROOM_ID, transaction.getRoomId());
        assertNotNull(transaction.getCreatedAt());
        assertTrue(transaction.isTransfer());
    }

    @Test
    void testCreateAtmRobbery() {
        BankTransaction transaction = BankTransaction.createAtmRobbery(USER_ID_1, AMOUNT, ROOM_ID);

        assertNull(transaction.getFromUserId());
        assertEquals(USER_ID_1, transaction.getToUserId());
        assertEquals(BankTransaction.TransactionType.ROBBERY, transaction.getTransactionType());
        assertEquals(AMOUNT, transaction.getAmount());
        assertEquals(BigDecimal.ZERO, transaction.getFeeAmount());
        assertEquals("ATM robbery", transaction.getDescription());
        assertEquals(ROOM_ID, transaction.getRoomId());
        assertNotNull(transaction.getCreatedAt());
        assertTrue(transaction.isRobbery());
    }

    @Test
    void testCreateAtmFee() {
        BankTransaction transaction = BankTransaction.createAtmFee(USER_ID_1, FEE, ROOM_ID);

        assertEquals(USER_ID_1, transaction.getFromUserId());
        assertNull(transaction.getToUserId());
        assertEquals(BankTransaction.TransactionType.ATM_FEE, transaction.getTransactionType());
        assertEquals(BigDecimal.ZERO, transaction.getAmount());
        assertEquals(FEE, transaction.getFeeAmount());
        assertEquals("ATM withdrawal fee", transaction.getDescription());
        assertEquals(ROOM_ID, transaction.getRoomId());
        assertNotNull(transaction.getCreatedAt());
        assertTrue(transaction.isAtmFee());
    }

    @Test
    void testTransactionTypeCheckers() {
        BankTransaction deposit = BankTransaction.createDeposit(USER_ID_1, AMOUNT, ROOM_ID);
        assertTrue(deposit.isDeposit());
        assertFalse(deposit.isWithdrawal());
        assertFalse(deposit.isTransfer());
        assertFalse(deposit.isRobbery());
        assertFalse(deposit.isAtmFee());

        BankTransaction withdrawal = BankTransaction.createWithdrawal(USER_ID_1, AMOUNT, FEE, ROOM_ID);
        assertFalse(withdrawal.isDeposit());
        assertTrue(withdrawal.isWithdrawal());
        assertFalse(withdrawal.isTransfer());
        assertFalse(withdrawal.isRobbery());
        assertFalse(withdrawal.isAtmFee());

        BankTransaction transfer = BankTransaction.createTransfer(USER_ID_1, USER_ID_2, AMOUNT, ROOM_ID);
        assertFalse(transfer.isDeposit());
        assertFalse(transfer.isWithdrawal());
        assertTrue(transfer.isTransfer());
        assertFalse(transfer.isRobbery());
        assertFalse(transfer.isAtmFee());

        BankTransaction robbery = BankTransaction.createAtmRobbery(USER_ID_1, AMOUNT, ROOM_ID);
        assertFalse(robbery.isDeposit());
        assertFalse(robbery.isWithdrawal());
        assertFalse(robbery.isTransfer());
        assertTrue(robbery.isRobbery());
        assertFalse(robbery.isAtmFee());

        BankTransaction fee = BankTransaction.createAtmFee(USER_ID_1, FEE, ROOM_ID);
        assertFalse(fee.isDeposit());
        assertFalse(fee.isWithdrawal());
        assertFalse(fee.isTransfer());
        assertFalse(fee.isRobbery());
        assertTrue(fee.isAtmFee());
    }

    @Test
    void testConstructorWithNullFee() {
        BankTransaction transaction = new BankTransaction(
                USER_ID_1, USER_ID_2, BankTransaction.TransactionType.TRANSFER,
                AMOUNT, null, "Test transaction", ROOM_ID
        );

        assertEquals(BigDecimal.ZERO, transaction.getFeeAmount());
    }

    @Test
    void testFullConstructor() {
        BankTransaction transaction = new BankTransaction(
                USER_ID_1, USER_ID_2, BankTransaction.TransactionType.TRANSFER,
                AMOUNT, FEE, "Test transaction", ROOM_ID
        );

        assertEquals(USER_ID_1, transaction.getFromUserId());
        assertEquals(USER_ID_2, transaction.getToUserId());
        assertEquals(BankTransaction.TransactionType.TRANSFER, transaction.getTransactionType());
        assertEquals(AMOUNT, transaction.getAmount());
        assertEquals(FEE, transaction.getFeeAmount());
        assertEquals("Test transaction", transaction.getDescription());
        assertEquals(ROOM_ID, transaction.getRoomId());
        assertNotNull(transaction.getCreatedAt());
    }
}