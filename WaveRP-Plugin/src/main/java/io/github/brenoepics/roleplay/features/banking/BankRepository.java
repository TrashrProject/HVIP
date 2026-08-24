package io.github.brenoepics.roleplay.features.banking;

import com.eu.habbo.Emulator;
import io.github.brenoepics.roleplay.features.banking.entities.ATMRobbery;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.banking.entities.BankTransaction;
import lombok.extern.slf4j.Slf4j;

import java.math.BigDecimal;
import java.sql.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

@Slf4j
public class BankRepository {

    // Bank Account Operations
    public Optional<BankAccount> findBankAccountByUserId(int userId) {
        String sql = "SELECT user_id, account_number, bank_balance, created_at, updated_at " +
                    "FROM bank_accounts WHERE user_id = ?";
        
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql)) {
            
            stmt.setInt(1, userId);
            try (ResultSet rs = stmt.executeQuery()) {
                if (rs.next()) {
                    return Optional.of(new BankAccount(
                        rs.getInt("user_id"),
                        rs.getString("account_number"),
                        rs.getBigDecimal("bank_balance"),
                        rs.getTimestamp("created_at"),
                        rs.getTimestamp("updated_at")
                    ));
                }
            }
        } catch (SQLException e) {
            log.error("Error finding bank account for user {}", userId, e);
        }
        return Optional.empty();
    }

    public boolean createBankAccount(BankAccount account) {
        String sql = "INSERT INTO bank_accounts (user_id, account_number, bank_balance) " +
                    "VALUES (?, ?, ?)";
        
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql)) {
            
            stmt.setInt(1, account.getUserId());
            stmt.setString(2, account.getAccountNumber());
            stmt.setBigDecimal(3, account.getBankBalance());
            
            return stmt.executeUpdate() > 0;
        } catch (SQLException e) {
            log.error("Error creating bank account for user {}", account.getUserId(), e);
            return false;
        }
    }

    public boolean updateBankAccount(BankAccount account) {
        String sql = "UPDATE bank_accounts SET bank_balance = ?, updated_at = CURRENT_TIMESTAMP " +
                    "WHERE user_id = ?";
        
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql)) {
            
            stmt.setBigDecimal(1, account.getBankBalance());
            stmt.setInt(2, account.getUserId());
            
            return stmt.executeUpdate() > 0;
        } catch (SQLException e) {
            log.error("Error updating bank account for user {}", account.getUserId(), e);
            return false;
        }
    }

    // Transaction Operations
    public boolean saveTransaction(BankTransaction transaction) {
        String sql = "INSERT INTO bank_transactions (from_user_id, to_user_id, transaction_type, amount, fee_amount, description, room_id) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)";
        
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)) {
            
            stmt.setObject(1, transaction.getFromUserId());
            stmt.setObject(2, transaction.getToUserId());
            stmt.setString(3, transaction.getTransactionType().name().toLowerCase());
            stmt.setBigDecimal(4, transaction.getAmount());
            stmt.setBigDecimal(5, transaction.getFeeAmount());
            stmt.setString(6, transaction.getDescription());
            stmt.setObject(7, transaction.getRoomId());
            
            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                try (ResultSet generatedKeys = stmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        transaction.setId(generatedKeys.getInt(1));
                    }
                }
                return true;
            }
        } catch (SQLException e) {
            log.error("Error saving transaction", e);
        }
        return false;
    }

    public List<BankTransaction> getTransactionsByUserId(int userId, int limit) {
        String sql = "SELECT id, from_user_id, to_user_id, transaction_type, amount, fee_amount, description, room_id, created_at " +
                    "FROM bank_transactions WHERE from_user_id = ? OR to_user_id = ? " +
                    "ORDER BY created_at DESC LIMIT ?";
        
        List<BankTransaction> transactions = new ArrayList<>();
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql)) {
            
            stmt.setInt(1, userId);
            stmt.setInt(2, userId);
            stmt.setInt(3, limit);
            
            try (ResultSet rs = stmt.executeQuery()) {
                while (rs.next()) {
                    BankTransaction transaction = new BankTransaction();
                    transaction.setId(rs.getInt("id"));
                    transaction.setFromUserId(rs.getObject("from_user_id", Integer.class));
                    transaction.setToUserId(rs.getObject("to_user_id", Integer.class));
                    transaction.setTransactionType(BankTransaction.TransactionType.valueOf(rs.getString("transaction_type").toUpperCase()));
                    transaction.setAmount(rs.getBigDecimal("amount"));
                    transaction.setFeeAmount(rs.getBigDecimal("fee_amount"));
                    transaction.setDescription(rs.getString("description"));
                    transaction.setRoomId(rs.getObject("room_id", Integer.class));
                    transaction.setCreatedAt(rs.getTimestamp("created_at"));
                    transactions.add(transaction);
                }
            }
        } catch (SQLException e) {
            log.error("Error getting transactions for user {}", userId, e);
        }
        return transactions;
    }

    // ATM Robbery Operations
    public boolean saveATMRobbery(ATMRobbery robbery) {
        String sql = "INSERT INTO atm_robberies (user_id, room_id, furni_id, amount_stolen, success, weapon_used, police_alerted) " +
                    "VALUES (?, ?, ?, ?, ?, ?, ?)";
        
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)) {
            
            stmt.setInt(1, robbery.getUserId());
            stmt.setInt(2, robbery.getRoomId());
            stmt.setInt(3, robbery.getFurniId());
            stmt.setBigDecimal(4, robbery.getAmountStolen());
            stmt.setBoolean(5, robbery.isSuccess());
            stmt.setString(6, robbery.getWeaponUsed());
            stmt.setBoolean(7, robbery.isPoliceAlerted());
            
            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                try (ResultSet generatedKeys = stmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        robbery.setId(generatedKeys.getInt(1));
                    }
                }
                return true;
            }
        } catch (SQLException e) {
            log.error("Error saving ATM robbery", e);
        }
        return false;
    }

    public List<ATMRobbery> getATMRobberiesByUserId(int userId, int limit) {
        String sql = "SELECT id, user_id, room_id, furni_id, amount_stolen, success, weapon_used, police_alerted, created_at " +
                    "FROM atm_robberies WHERE user_id = ? ORDER BY created_at DESC LIMIT ?";
        
        List<ATMRobbery> robberies = new ArrayList<>();
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement stmt = connection.prepareStatement(sql)) {
            
            stmt.setInt(1, userId);
            stmt.setInt(2, limit);
            
            try (ResultSet rs = stmt.executeQuery()) {
                while (rs.next()) {
                    ATMRobbery robbery = new ATMRobbery();
                    robbery.setId(rs.getInt("id"));
                    robbery.setUserId(rs.getInt("user_id"));
                    robbery.setRoomId(rs.getInt("room_id"));
                    robbery.setFurniId(rs.getInt("furni_id"));
                    robbery.setAmountStolen(rs.getBigDecimal("amount_stolen"));
                    robbery.setSuccess(rs.getBoolean("success"));
                    robbery.setWeaponUsed(rs.getString("weapon_used"));
                    robbery.setPoliceAlerted(rs.getBoolean("police_alerted"));
                    robbery.setCreatedAt(rs.getTimestamp("created_at"));
                    robberies.add(robbery);
                }
            }
        } catch (SQLException e) {
            log.error("Error getting ATM robberies for user {}", userId, e);
        }
        return robberies;
    }

    // Utility method to generate unique account numbers
    public String generateAccountNumber() {
        String sql = "SELECT account_number FROM bank_accounts WHERE account_number = ?";
        String accountNumber;
        
        do {
            // Generate a 10-digit account number
            accountNumber = String.valueOf(System.currentTimeMillis() % 10000000000L);
            // Pad with leading zeros if necessary
            accountNumber = String.format("%010d", Long.parseLong(accountNumber));
            
            try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
                 PreparedStatement stmt = connection.prepareStatement(sql)) {
                
                stmt.setString(1, accountNumber);
                try (ResultSet rs = stmt.executeQuery()) {
                    if (!rs.next()) {
                        break; // Account number is unique
                    }
                }
            } catch (SQLException e) {
                log.error("Error checking account number uniqueness", e);
                return null;
            }
        } while (true);
        
        return accountNumber;
    }
}