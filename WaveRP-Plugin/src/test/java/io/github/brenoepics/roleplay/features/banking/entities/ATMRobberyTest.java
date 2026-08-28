package io.github.brenoepics.roleplay.features.banking.entities;

import org.junit.jupiter.api.Test;

import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.*;

class ATMRobberyTest {

    private static final int USER_ID = 123;
    private static final int ROOM_ID = 456;
    private static final int FURNI_ID = 789;
    private static final BigDecimal AMOUNT_STOLEN = new BigDecimal("250.00");
    private static final String WEAPON_BAT = "Bat";
    private static final String WEAPON_NONE = "None";

    @Test
    void testCreateSuccessfulRobbery() {
        ATMRobbery robbery = ATMRobbery.createSuccessfulRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, WEAPON_BAT, true
        );

        assertEquals(USER_ID, robbery.getUserId());
        assertEquals(ROOM_ID, robbery.getRoomId());
        assertEquals(FURNI_ID, robbery.getFurniId());
        assertEquals(AMOUNT_STOLEN, robbery.getAmountStolen());
        assertTrue(robbery.isSuccess());
        assertEquals(WEAPON_BAT, robbery.getWeaponUsed());
        assertTrue(robbery.isPoliceAlerted());
        assertNotNull(robbery.getCreatedAt());
        assertTrue(robbery.isSuccessful());
        assertFalse(robbery.isFailed());
    }

    @Test
    void testCreateSuccessfulRobbery_NoPoliceAlert() {
        ATMRobbery robbery = ATMRobbery.createSuccessfulRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, WEAPON_BAT, false
        );

        assertTrue(robbery.isSuccess());
        assertFalse(robbery.isPoliceAlerted());
        assertTrue(robbery.isSuccessful());
        assertFalse(robbery.isFailed());
    }

    @Test
    void testCreateFailedRobbery() {
        ATMRobbery robbery = ATMRobbery.createFailedRobbery(
                USER_ID, ROOM_ID, FURNI_ID, WEAPON_NONE, true
        );

        assertEquals(USER_ID, robbery.getUserId());
        assertEquals(ROOM_ID, robbery.getRoomId());
        assertEquals(FURNI_ID, robbery.getFurniId());
        assertEquals(BigDecimal.ZERO, robbery.getAmountStolen());
        assertFalse(robbery.isSuccess());
        assertEquals(WEAPON_NONE, robbery.getWeaponUsed());
        assertTrue(robbery.isPoliceAlerted());
        assertNotNull(robbery.getCreatedAt());
        assertFalse(robbery.isSuccessful());
        assertTrue(robbery.isFailed());
    }

    @Test
    void testCreateFailedRobbery_NoPoliceAlert() {
        ATMRobbery robbery = ATMRobbery.createFailedRobbery(
                USER_ID, ROOM_ID, FURNI_ID, WEAPON_BAT, false
        );

        assertFalse(robbery.isSuccess());
        assertFalse(robbery.isPoliceAlerted());
        assertFalse(robbery.isSuccessful());
        assertTrue(robbery.isFailed());
    }

    @Test
    void testIsSuccessful_WithPositiveAmount() {
        ATMRobbery robbery = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, true, WEAPON_BAT, false
        );

        assertTrue(robbery.isSuccessful());
        assertFalse(robbery.isFailed());
    }

    @Test
    void testIsSuccessful_WithZeroAmount() {
        ATMRobbery robbery = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, BigDecimal.ZERO, true, WEAPON_BAT, false
        );

        assertFalse(robbery.isSuccessful());
        assertTrue(robbery.isFailed());
    }

    @Test
    void testIsFailed_SuccessFalse() {
        ATMRobbery robbery = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, false, WEAPON_BAT, false
        );

        assertTrue(robbery.isFailed());
        assertFalse(robbery.isSuccessful());
    }

    @Test
    void testConstructorWithNullAmount() {
        ATMRobbery robbery = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, null, true, WEAPON_BAT, false
        );

        assertEquals(BigDecimal.ZERO, robbery.getAmountStolen());
        assertFalse(robbery.isSuccessful());
        assertTrue(robbery.isFailed());
    }

    @Test
    void testFullConstructor() {
        ATMRobbery robbery = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, true, WEAPON_BAT, true
        );

        assertEquals(USER_ID, robbery.getUserId());
        assertEquals(ROOM_ID, robbery.getRoomId());
        assertEquals(FURNI_ID, robbery.getFurniId());
        assertEquals(AMOUNT_STOLEN, robbery.getAmountStolen());
        assertTrue(robbery.isSuccess());
        assertEquals(WEAPON_BAT, robbery.getWeaponUsed());
        assertTrue(robbery.isPoliceAlerted());
        assertNotNull(robbery.getCreatedAt());
    }

    @Test
    void testSuccessfulVsFailedLogic() {
        // Test edge cases for successful/failed logic
        
        // Success = true, amount > 0 -> successful
        ATMRobbery robbery1 = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, new BigDecimal("0.01"), true, WEAPON_BAT, false
        );
        assertTrue(robbery1.isSuccessful());
        assertFalse(robbery1.isFailed());

        // Success = true, amount = 0 -> failed
        ATMRobbery robbery2 = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, BigDecimal.ZERO, true, WEAPON_BAT, false
        );
        assertFalse(robbery2.isSuccessful());
        assertTrue(robbery2.isFailed());

        // Success = false, amount > 0 -> failed
        ATMRobbery robbery3 = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, false, WEAPON_BAT, false
        );
        assertFalse(robbery3.isSuccessful());
        assertTrue(robbery3.isFailed());

        // Success = false, amount = 0 -> failed
        ATMRobbery robbery4 = new ATMRobbery(
                USER_ID, ROOM_ID, FURNI_ID, BigDecimal.ZERO, false, WEAPON_BAT, false
        );
        assertFalse(robbery4.isSuccessful());
        assertTrue(robbery4.isFailed());
    }

    @Test
    void testWeaponVariations() {
        ATMRobbery robberyWithBat = ATMRobbery.createSuccessfulRobbery(
                USER_ID, ROOM_ID, FURNI_ID, AMOUNT_STOLEN, "Bat", false
        );
        assertEquals("Bat", robberyWithBat.getWeaponUsed());

        ATMRobbery robberyWithPistol = ATMRobbery.createFailedRobbery(
                USER_ID, ROOM_ID, FURNI_ID, "Pistol", true
        );
        assertEquals("Pistol", robberyWithPistol.getWeaponUsed());

        ATMRobbery robberyNoWeapon = ATMRobbery.createFailedRobbery(
                USER_ID, ROOM_ID, FURNI_ID, null, false
        );
        assertNull(robberyNoWeapon.getWeaponUsed());
    }
}