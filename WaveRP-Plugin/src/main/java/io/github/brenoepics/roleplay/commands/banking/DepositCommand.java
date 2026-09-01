package io.github.brenoepics.roleplay.commands.banking;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.HabboItem;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.banking.BankManager;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionATM;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.math.BigDecimal;

public class DepositCommand extends Command {

  public DepositCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
    if (data.isPassive()) {
      gameClient.getHabbo()
          .whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.",
              RoomChatMessageBubbles.ALERT);
      return true;
    }

    boolean mobile = params.length == 3 && "phone".equalsIgnoreCase(params[2]);
    if (params.length != 2 && !mobile) {
      gameClient.getHabbo().whisper(":deposer <montant>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    String amountStr = params[1];

    // Parse amount
    BigDecimal amount;
    try {
      amount = new BigDecimal(amountStr);
      if (amount.compareTo(BigDecimal.ZERO) <= 0) {
        gameClient.getHabbo()
            .whisper(BankManager.ERROR_INVALID_AMOUNT, RoomChatMessageBubbles.ALERT);
        return true;
      }
    } catch (NumberFormatException e) {
      gameClient.getHabbo().whisper(BankManager.ERROR_INVALID_AMOUNT, RoomChatMessageBubbles.ALERT);
      return true;
    }

    BankManager bankManager = RolePlay.getBankManager();
    int userId = gameClient.getHabbo().getHabboInfo().getId();

    // Check if user has bank account
    if (!bankManager.hasBankAccount(userId)) {
      gameClient.getHabbo()
          .whisper(BankManager.ERROR_NO_BANK_ACCOUNT, RoomChatMessageBubbles.ALERT);
      return true;
    }

    // Check if user is near an ATM
    Room currentRoom = gameClient.getHabbo().getHabboInfo().getCurrentRoom();
    if (currentRoom == null) {
      gameClient.getHabbo()
          .whisper("Vous devez \u00eatre dans une salle pour utiliser les commandes bancaires.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    boolean nearATM = isNearATM(currentRoom, gameClient.getHabbo());
    if (!mobile && !nearATM) {
      gameClient.getHabbo()
          .whisper("Vous devez \u00eatre pr\u00e8s d'un distributeur pour effectuer un d\u00e9p\u00f4t.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    // Validate deposit
    if (!bankManager.canDeposit(userId, amount)) {
      gameClient.getHabbo()
          .whisper(BankManager.ERROR_INSUFFICIENT_WALLET, RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (mobile) {
      long remaining = bankManager.getMobileDepositCooldownSeconds(userId);
      if (remaining > 0) {
        long minutes = Math.max(1, (remaining + 59) / 60);
        gameClient.getHabbo().whisper("Le dépôt mobile est limité à un toutes les 30 minutes. Réessayez dans " + minutes + " minute(s), ou rendez-vous au guichet.", RoomChatMessageBubbles.ALERT);
        return true;
      }
    }

    // Perform deposit
    int roomId = currentRoom.getId();
    boolean success = mobile ? bankManager.mobileDeposit(userId, amount, roomId) : bankManager.deposit(userId, amount, roomId);
    if (success) {
      String formattedAmount = bankManager.formatCurrency(amount);
      String successMessage = String.format(BankManager.SUCCESS_DEPOSIT, formattedAmount);
      gameClient.getHabbo().whisper(successMessage, RoomChatMessageBubbles.ALERT);
      if (mobile) gameClient.getHabbo().whisper("Prochain dépôt mobile disponible dans 30 minutes. Les dépôts au guichet restent disponibles.", RoomChatMessageBubbles.NORMAL);
    } else {
      gameClient.getHabbo()
          .whisper("Le d\u00e9p\u00f4t a \u00e9chou\u00e9. R\u00e9essayez plus tard.", RoomChatMessageBubbles.ALERT);
    }

    return true;
  }

  /**
   * Check if user is near an ATM in the current room
   */
  private boolean isNearATM(Room room, com.eu.habbo.habbohotel.users.Habbo habbo) {
    // Check all furniture items in the room for ATMs
    for (HabboItem item : room.getFloorItems()) {
      // Check if this item is an ATM (you might need to check by base item ID or interaction type)
      if (isATMItem(item)) {
        if (InteractionATM.isUserAroundATM(habbo, room, item)) {
          return true;
        }
      }
    }
    return false;
  }

  /**
   * Check if a furniture item is an ATM This can be enhanced to check against specific ATM
   * furniture IDs
   */
  private boolean isATMItem(HabboItem item) {
    return item.getBaseItem().getInteractionType().getType().equals(InteractionATM.class);
  }
}
