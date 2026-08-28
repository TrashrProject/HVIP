package io.github.brenoepics.roleplay.features.banking.entities;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.*;

class BankAccountTest {
    
    private BankAccount bankAccount;
    private static final int USER_ID = 123;
    private static final String ACCOUNT_NUMBER = "1234567890";
    private static final BigDecimal INITIAL_BANK = new BigDecimal("50.00");
    private static final BigDecimal ATM_FEE = new BigDecimal("0.05"); // 5%

    @BeforeEach
    void setUp() {
        bankAccount = new BankAccount(USER_ID, ACCOUNT_NUMBER);
        bankAccount.setBankBalance(INITIAL_BANK);
    }

    @Test
    void testConstructorWithUserIdAndAccountNumber() {
        BankAccount account = new BankAccount(USER_ID, ACCOUNT_NUMBER);
        assertEquals(USER_ID, account.getUserId());
        assertEquals(ACCOUNT_NUMBER, account.getAccountNumber());
        assertEquals(BigDecimal.ZERO, account.getBankBalance());
        assertNotNull(account.getCreatedAt());
        assertNotNull(account.getUpdatedAt());
    }

    @Test
    void testHasEnoughBankBalance() {
        assertTrue(bankAccount.hasEnoughBankBalance(new BigDecimal("25.00")));
        assertTrue(bankAccount.hasEnoughBankBalance(new BigDecimal("50.00")));
        assertFalse(bankAccount.hasEnoughBankBalance(new BigDecimal("50.01")));
        assertFalse(bankAccount.hasEnoughBankBalance(new BigDecimal("100.00")));
    }

    @Test
    void testDepositToBank_Success() {
        BigDecimal depositAmount = new BigDecimal("30.00");
        BigDecimal expectedBank = INITIAL_BANK.add(depositAmount);

        bankAccount.depositToBank(depositAmount);

        assertEquals(expectedBank, bankAccount.getBankBalance());
    }

    @Test
    void testDepositToBank_NegativeAmount() {
        BigDecimal depositAmount = new BigDecimal("-10.00");

        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.depositToBank(depositAmount);
        });
    }

    @Test
    void testDepositToBank_ZeroAmount() {
        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.depositToBank(BigDecimal.ZERO);
        });
    }

    @Test
    void testWithdrawFromBank_Success() {
        BigDecimal withdrawAmount = new BigDecimal("40.00");
        BigDecimal expectedFee = withdrawAmount.multiply(ATM_FEE); // $2.00
        BigDecimal expectedBank = INITIAL_BANK.subtract(withdrawAmount);

        BigDecimal actualFee = bankAccount.withdrawFromBank(withdrawAmount, ATM_FEE);

        assertEquals(expectedFee, actualFee);
        assertEquals(expectedBank, bankAccount.getBankBalance());
    }

    @Test
    void testWithdrawFromBank_InsufficientBankBalance() {
        BigDecimal withdrawAmount = new BigDecimal("100.00");

        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.withdrawFromBank(withdrawAmount, ATM_FEE);
        });

        // Balance should remain unchanged
        assertEquals(INITIAL_BANK, bankAccount.getBankBalance());
    }

    @Test
    void testWithdrawFromBank_NegativeAmount() {
        BigDecimal withdrawAmount = new BigDecimal("-10.00");

        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.withdrawFromBank(withdrawAmount, ATM_FEE);
        });
    }

    @Test
    void testAddToBankBalance_Success() {
        BigDecimal addAmount = new BigDecimal("25.00");
        BigDecimal expectedBank = INITIAL_BANK.add(addAmount);

        bankAccount.addToBankBalance(addAmount);

        assertEquals(expectedBank, bankAccount.getBankBalance());
    }

    @Test
    void testAddToBankBalance_NegativeAmount() {
        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.addToBankBalance(new BigDecimal("-10.00"));
        });
    }

    @Test
    void testSubtractFromBankBalance_Success() {
        BigDecimal subtractAmount = new BigDecimal("30.00");
        BigDecimal expectedBank = INITIAL_BANK.subtract(subtractAmount);

        bankAccount.subtractFromBankBalance(subtractAmount);

        assertEquals(expectedBank, bankAccount.getBankBalance());
    }

    @Test
    void testSubtractFromBankBalance_InsufficientBalance() {
        BigDecimal subtractAmount = new BigDecimal("100.00");

        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.subtractFromBankBalance(subtractAmount);
        });

        // Balance should remain unchanged
        assertEquals(INITIAL_BANK, bankAccount.getBankBalance());
    }

    @Test
    void testSubtractFromBankBalance_NegativeAmount() {
        assertThrows(IllegalArgumentException.class, () -> {
            bankAccount.subtractFromBankBalance(new BigDecimal("-10.00"));
        });
    }
}