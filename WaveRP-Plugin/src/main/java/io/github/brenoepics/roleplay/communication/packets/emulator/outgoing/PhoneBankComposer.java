package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;

import java.math.BigDecimal;
import java.util.List;

/**
 * Phone Bank full composer (snapshot + recent transactions)
 * Packet ID: 6007
 * Structure:
 *  bool has_account
 *  string account_number (empty if none)
 *  string bank_balance (plain string decimal)
 *  string wallet_balance
 *  string total_balance
 *  string fee_percentage (e.g. 0.05)
 *  bool can_deposit
 *  bool can_withdraw
 *  string updated_at_ms
 *  int transaction_count
 *      For each transaction:
 *          int id
 *          string type (DEPOSIT|WITHDRAW|TRANSFER|ATM_FEE|ROBBERY)
 *          string amount
 *          string fee_amount
 *          int from_user_id (-1 if null)
 *          int to_user_id (-1 if null)
 *          int room_id (-1 if null)
 *          string description
 *          string created_at_ms
 */
public class PhoneBankComposer extends MessageComposer {

    private final boolean hasAccount;
    private final String accountNumber;
    private final BigDecimal bankBalance;
    private final BigDecimal walletBalance;
    private final BigDecimal feePercentage;
    private final boolean canDeposit;
    private final boolean canWithdraw;
    private final long updatedAt;
    private final List<BankTransaction> transactions;

    public PhoneBankComposer(boolean hasAccount,
                             String accountNumber,
                             BigDecimal bankBalance,
                             BigDecimal walletBalance,
                             BigDecimal feePercentage,
                             boolean canDeposit,
                             boolean canWithdraw,
                             List<BankTransaction> transactions) {
        this.hasAccount = hasAccount;
        this.accountNumber = accountNumber == null ? "" : accountNumber;
        this.bankBalance = bankBalance == null ? BigDecimal.ZERO : bankBalance;
        this.walletBalance = walletBalance == null ? BigDecimal.ZERO : walletBalance;
        this.feePercentage = feePercentage == null ? BigDecimal.ZERO : feePercentage;
        this.canDeposit = canDeposit;
        this.canWithdraw = canWithdraw;
        this.transactions = transactions;
        this.updatedAt = System.currentTimeMillis();
    }

    @Override
    protected ServerMessage composeInternal() {
        this.response.init(6007);
        this.response.appendBoolean(this.hasAccount);
        this.response.appendString(this.accountNumber);
        this.response.appendString(this.bankBalance.toPlainString());
        this.response.appendString(this.walletBalance.toPlainString());
        this.response.appendString(this.bankBalance.add(this.walletBalance).toPlainString());
        this.response.appendString(this.feePercentage.toPlainString());
        this.response.appendBoolean(this.canDeposit);
        this.response.appendBoolean(this.canWithdraw);
        this.response.appendString(Long.toString(this.updatedAt));

        int count = this.transactions == null ? 0 : this.transactions.size();
        this.response.appendInt(count);
        if (count > 0) {
            for (BankTransaction t : this.transactions) {
                this.response.appendInt(t.getId());
                this.response.appendString(t.getTransactionType().name());
                this.response.appendString(t.getAmount() == null ? "0" : t.getAmount().toPlainString());
                this.response.appendString(t.getFeeAmount() == null ? "0" : t.getFeeAmount().toPlainString());
                this.response.appendInt(t.getFromUserId() == null ? -1 : t.getFromUserId());
                this.response.appendInt(t.getToUserId() == null ? -1 : t.getToUserId());
                this.response.appendInt(t.getRoomId() == null ? -1 : t.getRoomId());
                this.response.appendString(t.getDescription() == null ? "" : t.getDescription());
                this.response.appendString(Long.toString(t.getCreatedAt() == null ? 0L : t.getCreatedAt().getTime()));
            }
        }
        return this.response;
    }
}

