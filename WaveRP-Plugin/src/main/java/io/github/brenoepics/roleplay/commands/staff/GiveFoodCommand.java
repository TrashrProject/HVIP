package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.generic.GenericAlertComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.util.Comparator;
import java.util.Optional;

public class GiveFoodCommand extends Command {

  private static final int MINIMUM_STAFF_RANK = 5;
  private static final int MAXIMUM_STAFF_RANK = 9;
  private static final int MAX_QUANTITY = 100;
  private static final String MENU_MARKER = "PARADISE_FOOD_MENU";

  public GiveFoodCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo staff = gameClient.getHabbo();
    int staffRank = staff.getHabboInfo().getRank().getId();

    if (staffRank < MINIMUM_STAFF_RANK || staffRank > MAXIMUM_STAFF_RANK) {
      sendNoPermission(staff);
      return true;
    }

    if (params.length < 2) {
      openFoodMenu(gameClient);
      return true;
    }

    int quantity = 1;
    int itemEndIndex = params.length;

    if (params.length >= 3) {
      try {
        quantity = Integer.parseInt(params[params.length - 1]);
        itemEndIndex = params.length - 1;
      } catch (NumberFormatException ignored) {
        quantity = 1;
      }
    }

    if (quantity < 1 || quantity > MAX_QUANTITY) {
      staff.whisper("La quantité doit être comprise entre 1 et " + MAX_QUANTITY + ".",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    StringBuilder itemInputBuilder = new StringBuilder();
    for (int i = 1; i < itemEndIndex; i++) {
      if (itemInputBuilder.length() > 0) {
        itemInputBuilder.append(' ');
      }
      itemInputBuilder.append(params[i]);
    }

    String itemInput = itemInputBuilder.toString().trim();
    RPItem food = findFood(itemInput).orElse(null);
    if (food == null) {
      staff.whisper("Cette nourriture n'existe pas. Utilise :food pour voir la liste.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(staff);
    if (avatar == null || avatar.getInventory() == null) {
      staff.whisper("Impossible de charger ton inventaire RP.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    avatar.getInventory().addItem(staff, food, quantity);
    staff.whisper("Tu as reçu " + quantity + " x " + food.getDisplayName() + ".",
        RoomChatMessageBubbles.NORMAL);
    return true;
  }

  private static void openFoodMenu(GameClient gameClient) {
    StringBuilder payload = new StringBuilder(MENU_MARKER).append('\n');
    RolePlay.getItemManager().getItems().values().stream()
        .filter(GiveFoodCommand::isFood)
        .sorted(Comparator.comparingInt(RPItem::getId))
        .forEach(item -> payload.append("FOOD|")
            .append(item.getId()).append('|')
            .append(sanitize(item.getDisplayName())).append('|')
            .append(sanitize(item.getExtraData())).append('\n'));

    gameClient.sendResponse(new GenericAlertComposer(payload.toString()));
  }

  private static String sanitize(String value) {
    return value == null ? "" : value.replace("|", " ").replace("\r", " ").replace("\n", " ");
  }

  public boolean handlePermissionDenied(GameClient gameClient, String[] params) {
    sendNoPermission(gameClient.getHabbo());
    return true;
  }

  private static Optional<RPItem> findFood(String input) {
    try {
      int itemId = Integer.parseInt(input);
      RPItem item = RolePlay.getItemManager().getItems().get(itemId);
      return isFood(item) ? Optional.of(item) : Optional.empty();
    } catch (NumberFormatException ignored) {
      return RolePlay.getItemManager().getItems().values().stream()
          .filter(GiveFoodCommand::isFood)
          .filter(item -> item.getDisplayName().equalsIgnoreCase(input)
              || item.getRawDisplayName().equalsIgnoreCase(input))
          .findFirst();
    }
  }

  private static boolean isFood(RPItem item) {
    return item != null && "food".equalsIgnoreCase(item.getInteractionType());
  }

  private static void sendNoPermission(Habbo habbo) {
    habbo.whisper("Vous n'avez pas la permission d'utiliser cette commande.",
        RoomChatMessageBubbles.ALERT);
  }
}
