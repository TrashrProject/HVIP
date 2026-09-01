package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.banking.entities.BankAccount;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.AtmBankComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.ClientLinkEventComposer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.math.BigDecimal;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Optional;
import java.util.concurrent.ConcurrentHashMap;

public class InteractionATM extends InteractionDefault {

    private static final Logger log = LoggerFactory.getLogger(InteractionATM.class);
    private static final int MAX_INTERACTION_DISTANCE = 2; // Maximum distance to interact with ATM
    private static final ConcurrentHashMap<Integer, Long> LAST_OPEN = new ConcurrentHashMap<>();

    public InteractionATM(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public InteractionATM(int id, int userId, Item item, String extradata, int limitedStack, int limitedSells) {
        super(id, userId, item, extradata, limitedStack, limitedSells);
    }

    @Override
    public boolean isUsable() {
        return true;
    }

    @Override
    public boolean allowWiredResetState() {
        return false;
    }

    @Override
    public void onClick(GameClient client, Room room, Object[] objects) {
        if (client.getHabbo() == null || room == null) {
            return;
        }

        Habbo habbo = client.getHabbo();
        long now = System.currentTimeMillis();
        Long previous = LAST_OPEN.put(habbo.getHabboInfo().getId(), now);
        if (previous != null && now - previous < 750L) return;
        RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
        
        // Check if user is in passive mode
        if (rpAvatar.isPassive()) {
            habbo.whisper("Vous ne pouvez pas utiliser le distributeur en mode passif !", RoomChatMessageBubbles.ALERT);
            return;
        }

        // Check proximity - user must be close to the ATM
        if (!isUserInRange(habbo, room)) {
            habbo.whisper("Vous devez vous rapprocher du distributeur pour l'utiliser !", RoomChatMessageBubbles.ALERT);
            return;
        }

        BankManager bankManager = RolePlay.getBankManager();
        int userId = habbo.getHabboInfo().getId();
        
        // Check if user has bank account
        if (!bankManager.hasBankAccount(userId)) {
            habbo.whisper(BankManager.ERROR_NO_BANK_ACCOUNT, RoomChatMessageBubbles.ALERT);
            return;
        }

        // Get bank account and send ATM composer
        Optional<BankAccount> accountOpt = bankManager.getBankAccount(userId);
        if (accountOpt.isEmpty()) {
            habbo.whisper("Impossible d'accéder à votre compte bancaire. Réessayez plus tard.", RoomChatMessageBubbles.ALERT);
            return;
        }

        BankAccount account = accountOpt.get();
        BigDecimal bankBalance = account.getBankBalance();
        int walletInt = 0;
        try {
            walletInt = habbo.getHabboInfo().getCredits();
        } catch (Exception ignored) {}
        BigDecimal walletBalance = BigDecimal.valueOf(walletInt);
        BigDecimal feePercentage = bankManager.getATMFeePercentage();
        boolean canDeposit = walletBalance.compareTo(BigDecimal.ZERO) > 0;
        boolean canWithdraw = bankBalance.compareTo(BigDecimal.ZERO) > 0;

        // Send only the ATM snapshot composer (UI should open / refresh ATM panel)
        habbo.getClient().sendResponse(new AtmBankComposer(
                true,
                account.getAccountNumber(),
                bankBalance,
                walletBalance,
                feePercentage,
                canDeposit,
                canWithdraw
        ));
        habbo.getClient().sendResponse(new ClientLinkEventComposer("atm/show"));
        habbo.shout("* Utilise le distributeur automatique *", RoomChatMessageBubbles.NORMAL);

        log.info("User {} accessed ATM {} in room {} - Balance total:{}",
                userId, this.getId(), room.getId(), bankManager.formatCurrency(bankBalance.add(walletBalance)));
    }

    /**
     * Checks if the user is within interaction range of the ATM
     */
    private boolean isUserInRange(Habbo habbo, Room room) {
        if (habbo.getRoomUnit() == null) {
            return false;
        }

        RoomTile userTile = habbo.getRoomUnit().getCurrentLocation();
        RoomTile atmTile = room.getLayout().getTile((short) this.getX(), (short) this.getY());
        
        if (userTile == null || atmTile == null) {
            return false;
        }

        // Calculate distance between user and ATM
        double distance = Math.sqrt(Math.pow(userTile.x - atmTile.x, 2) + Math.pow(userTile.y - atmTile.y, 2));
        return distance <= MAX_INTERACTION_DISTANCE;
    }

    /**
     * Check if user is around the ATM for security
     */
    public static boolean isUserAroundATM(Habbo habbo, Room room, HabboItem atmItem) {
        if (habbo.getRoomUnit() == null || atmItem == null) {
            return false;
        }

        RoomTile userTile = habbo.getRoomUnit().getCurrentLocation();
        RoomTile atmTile = room.getLayout().getTile((short) atmItem.getX(), (short) atmItem.getY());
        
        if (userTile == null || atmTile == null) {
            return false;
        }

        // Calculate distance between user and ATM
        double distance = Math.sqrt(Math.pow(userTile.x - atmTile.x, 2) + Math.pow(userTile.y - atmTile.y, 2));
        return distance <= MAX_INTERACTION_DISTANCE;
    }

    @Override
    public void onPickUp(Room room) {
        super.onPickUp(room);
    }
}
