package io.github.brenoepics.roleplay.features.banking.entities;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.math.BigDecimal;
import java.sql.Timestamp;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class ATMRobbery {
    private int id;
    private int userId;
    private int roomId;
    private int furniId;
    private BigDecimal amountStolen;
    private boolean success;
    private String weaponUsed;
    private boolean policeAlerted;
    private Timestamp createdAt;

    public ATMRobbery(int userId, int roomId, int furniId, BigDecimal amountStolen, 
                     boolean success, String weaponUsed, boolean policeAlerted) {
        this.userId = userId;
        this.roomId = roomId;
        this.furniId = furniId;
        this.amountStolen = amountStolen != null ? amountStolen : BigDecimal.ZERO;
        this.success = success;
        this.weaponUsed = weaponUsed;
        this.policeAlerted = policeAlerted;
        this.createdAt = new Timestamp(System.currentTimeMillis());
    }

    public static ATMRobbery createSuccessfulRobbery(int userId, int roomId, int furniId, 
                                                   BigDecimal amountStolen, String weaponUsed, 
                                                   boolean policeAlerted) {
        return new ATMRobbery(userId, roomId, furniId, amountStolen, true, weaponUsed, policeAlerted);
    }

    public static ATMRobbery createFailedRobbery(int userId, int roomId, int furniId, 
                                               String weaponUsed, boolean policeAlerted) {
        return new ATMRobbery(userId, roomId, furniId, BigDecimal.ZERO, false, weaponUsed, policeAlerted);
    }

    public boolean isSuccessful() {
        return success && amountStolen.compareTo(BigDecimal.ZERO) > 0;
    }

    public boolean isFailed() {
        return !success || amountStolen.compareTo(BigDecimal.ZERO) == 0;
    }
}