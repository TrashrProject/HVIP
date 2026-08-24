package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;

import java.math.BigDecimal;

/**
 * ATM Bank snapshot composer.
 * Packet ID: 6006 (ensure it does not conflict with base emulator).
 * Order:
 *  [0] has_account (bool)
 *  [1] account_number (string, empty if none)
 *  [2] bank_balance (string decimal)
 *  [3] wallet_balance (string decimal)
 *  [4] total_balance (string decimal)
 *  [5] fee_percentage (string decimal, e.g. 0.05)
 *  [6] can_deposit (bool)
 *  [7] can_withdraw (bool)
 *  [8] updated_at_ms (string millis)
 */
public class AtmBankComposer extends MessageComposer {

    private final boolean hasAccount;
    private final String accountNumber;
    private final BigDecimal bankBalance;
    private final BigDecimal walletBalance;
    private final BigDecimal feePercentage;
    private final boolean canDeposit;
    private final boolean canWithdraw;
    private final long updatedAt;

    public AtmBankComposer(boolean hasAccount,
                           String accountNumber,
                           BigDecimal bankBalance,
                           BigDecimal walletBalance,
                           BigDecimal feePercentage,
                           boolean canDeposit,
                           boolean canWithdraw) {
        this.hasAccount = hasAccount;
        this.accountNumber = accountNumber == null ? "" : accountNumber;
        this.bankBalance = bankBalance == null ? BigDecimal.ZERO : bankBalance;
        this.walletBalance = walletBalance == null ? BigDecimal.ZERO : walletBalance;
        this.feePercentage = feePercentage == null ? BigDecimal.ZERO : feePercentage;
        this.canDeposit = canDeposit;
        this.canWithdraw = canWithdraw;
        this.updatedAt = System.currentTimeMillis();
    }

    @Override
    protected ServerMessage composeInternal() {
        this.response.init(6006);
        this.response.appendBoolean(this.hasAccount);
        this.response.appendString(this.accountNumber);
        this.response.appendString(this.bankBalance.toPlainString());
        this.response.appendString(this.walletBalance.toPlainString());
        this.response.appendString(this.bankBalance.add(this.walletBalance).toPlainString());
        this.response.appendString(this.feePercentage.toPlainString());
        this.response.appendBoolean(this.canDeposit);
        this.response.appendBoolean(this.canWithdraw);
        this.response.appendString(Long.toString(this.updatedAt));
        return this.response;
    }
}

