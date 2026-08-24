package io.github.brenoepics.roleplay.features.offer;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.OfferComposer;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.util.Map;
import java.util.Random;

public class OfferManager {

  public record Offer(Habbo habbo, Integer itemId, boolean isSell) {

  }

  private final THashMap<Integer, Map<String, Offer>> offers = new THashMap<>();

  public void acceptOffer(Habbo habbo, String code) {
    Map<String, Offer> userOffers = getUserOffers(habbo.getHabboInfo().getId());
    if (userOffers == null || userOffers.isEmpty()) {
      habbo.whisper("No offer was made to you", RoomChatMessageBubbles.ALERT);
      return;
    }

    Offer offer = userOffers.get(code);
    if (offer == null) {
      habbo.whisper("The offer code is invalid", RoomChatMessageBubbles.ALERT);
      return;
    }

    RpAvatar habboData = RolePlay.getAvatarManager().getRpAvatar(habbo);
    RpAvatar offerData = RolePlay.getAvatarManager().getRpAvatar(offer.habbo());
    if (habboData == null || offerData == null) {
      habbo.whisper("An error occurred while accepting the offer", RoomChatMessageBubbles.ALERT);
      userOffers.remove(code);
      return;
    }

    RPItem item = offer.isSell() ? offerData.getInventory().getSlotItem(offer.itemId())
        : RolePlay.getItemManager().getItemById(offer.itemId());
    if (item == null) {
      habbo.whisper("The offer item was not found", RoomChatMessageBubbles.ALERT);
      userOffers.remove(code);
      return;
    }

    if (habbo.getHabboInfo().getCurrencyAmount(200) < item.getPrice()) {
      habbo.whisper("You do not have enough Bucks", RoomChatMessageBubbles.ALERT);
      userOffers.remove(code);
      return;
    }

    habbo.getHabboInfo()
        .setCurrencyAmount(200, habbo.getHabboInfo().getCurrencyAmount(200) - item.getPrice());
    if (offer.isSell()) {
      offer.habbo.getHabboInfo().addCurrencyAmount(200, item.getPrice());
      offerData.getInventory().removeItem(item, 1);
    }

    habboData.getInventory().addItem(habbo, item, 1);
    offerData.getInventory().updateInventory(offer.habbo);
    habbo.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(
        new RoomChatMessage(
            "Accepted the offer from " + offer.habbo().getHabboInfo().getUsername() + "*", habbo,
            habbo, RoomChatMessageBubbles.NORMAL)).compose());
    userOffers.remove(code);
  }

  public void clearOffers(Habbo habbo) {
    this.offers.values().forEach(o -> o.values().removeIf(offer -> offer.habbo() == habbo));
  }

  public boolean handleOfferItem(String itemName, RpAvatar offeringData, Habbo offering,
      Habbo receiver) {
    RPItem item = RolePlay.getItemManager().getItemByName(itemName);
    if (item == null) {
      offering.whisper("Item not found", RoomChatMessageBubbles.ALERT);
      return false;
    }

    if (item.getOfferJob() != null && item.getOfferJob() != offeringData.getJobEntity()) {
      offering.whisper(
          "You must be from the " + item.getOfferJob().getName() + " to offer this item",
          RoomChatMessageBubbles.ALERT);
      return false;
    }
    if (item.getOfferJob() == null) {
      offering.whisper("This item cannot be offered", RoomChatMessageBubbles.ALERT);
      return false;
    }
    Organization organization = RolePlay.getOrganizationManager()
        .getOrganization(offeringData.getOrganizationId());
    if (!item.getCrafterOrganizations().isEmpty() && (organization == null
        || !item.getCrafterOrganizations().contains(organization.getType()))) {
      offering.whisper("Your organization cannot craft this item", RoomChatMessageBubbles.ALERT);
      return false;
    }

    // Required handitem check
    if (item.getRequiredHanditem() > 0) {
      int hand = offering.getRoomUnit().getHandItem();
      if (hand != item.getRequiredHanditem()) {
        offering.whisper("You must be holding the required item to offer this.", RoomChatMessageBubbles.ALERT);
        return false;
      }
    }

    // Proximity check: must be exactly 1 tile away
    if (!validateOfferProximity(offering, receiver)) {
      return false;
    }

    String message = buildMessage(receiver.getHabboInfo().getUsername(), item,
        String.valueOf(item.getPrice()));

    String offerCode = getRandomString(4);
    offering.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(
        new RoomChatMessage(message, offering, offering, RoomChatMessageBubbles.NORMAL)).compose());
    createOffer(receiver.getHabboInfo().getId(), new Offer(offering, item.getId(), false),
        offerCode);
    receiver.whisper(
        "You have received an offer from " + offering.getHabboInfo().getUsername() + " for "
            + item.getDisplayName() + " for " + item.getPrice() + " bucks. Use :acceptoffer "
            + offerCode + " to accept it", RoomChatMessageBubbles.NORMAL);
    OfferComposer offerComposer = new OfferComposer(offering, item, offerCode);
    receiver.getClient().sendResponse(offerComposer);
    return true;
  }

  public String buildMessage(String username, RPItem item, String price) {
    String message = Emulator.getTexts().getValue("features.offer." + item.getId() + ".message",
        "Offers %habbo% a %item% for %price% bucks.*");
    message = message.replace("%habbo%", username);
    message = message.replace("%item%", item.getDisplayName());
    return message.replace("%price%", price);
  }

  public void handleSellItem(RPItem item, RpAvatar data, Habbo offering, Habbo receiving) {
    
    if (item.getRequirementJob() != null && item.getRequirementJob() != data.getJobEntity()) {
      offering.whisper(
          "You must be a " + item.getRequirementJob().getName() + " to offer this item",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    if (item.getOfferJob() != null && item.getOfferJob() != data.getJobEntity()) {
      offering.whisper(
          "You must be from the " + item.getOfferJob().getName() + " to offer this item",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    Organization organization = RolePlay.getOrganizationManager()
        .getOrganization(data.getOrganizationId());
    if (!item.getCrafterOrganizations().isEmpty() && (organization == null
        || !item.getCrafterOrganizations().contains(organization.getType()))) {
      offering.whisper("Your organization cannot craft this item", RoomChatMessageBubbles.ALERT);
      return;
    }

    // Required handitem check
    if (item.getRequiredHanditem() > 0) {
      int hand = offering.getRoomUnit().getHandItem();
      if (hand != item.getRequiredHanditem()) {
        offering.whisper("You must be holding the required item to offer this.", RoomChatMessageBubbles.ALERT);
        return;
      }
    }

    if (!validateOfferProximity(offering, receiving)) {
      return;
    }
    String message = buildMessage(offering.getHabboInfo().getUsername(), item,
        String.valueOf(item.getPrice()));
    String offerCode = getRandomString(4);
    offering.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(
        new RoomChatMessage(message, offering, offering, RoomChatMessageBubbles.NORMAL)).compose());
    createOffer(receiving.getHabboInfo().getId(), new Offer(offering, item.getId(), true),
        offerCode);
    receiving.whisper(
        "You have received an offer from " + offering.getHabboInfo().getUsername() + " for "
            + item.getDisplayName() + " for " + item.getPrice() + " bucks. Use :acceptoffer "
            + offerCode + " to accept it", RoomChatMessageBubbles.NORMAL);
    OfferComposer offerComposer = new OfferComposer(offering, item, offerCode);
    receiving.getClient().sendResponse(offerComposer);
  }


  private boolean validateOfferProximity(Habbo offering, Habbo receiver) {
    if (offering == null || receiver == null) {
      return false;
    }
    if (offering.getHabboInfo().getCurrentRoom() == null
        || receiver.getHabboInfo().getCurrentRoom() == null
        || offering.getHabboInfo().getCurrentRoom() != receiver.getHabboInfo().getCurrentRoom()) {
      offering.whisper("This user is too far away!", RoomChatMessageBubbles.ALERT);
      return false;
    }
    RoomTile oTile = offering.getRoomUnit().getCurrentLocation();
    RoomTile rTile = receiver.getRoomUnit().getCurrentLocation();
    if (oTile == null || rTile == null) {
      offering.whisper("This user is too far away!", RoomChatMessageBubbles.ALERT);
      return false;
    }
    double distance = oTile.distance(rTile);
    if (distance < 1.0) {
      offering.whisper("You must be at least 1 tile away!", RoomChatMessageBubbles.ALERT);
      return false;
    }
    if (distance > 1.0) {
      offering.whisper("This user is too far away!", RoomChatMessageBubbles.ALERT);
      return false;
    }
    return true;
  }

  public static String getRandomString(int length) {
    String chars = "1234567890";
    StringBuilder salt = new StringBuilder();
    Random rnd = Emulator.getRandom();
    while (salt.length() < length) {
      int index = (int) (rnd.nextFloat() * chars.length());
      salt.append(chars.charAt(index));
    }
    return salt.toString();

  }

  public boolean declineOffer(Integer userId, String code) {
    return getUserOffers(userId).remove(code) != null;
  }

  public Map<String, Offer> getUserOffers(int userId) {
    return this.offers.computeIfAbsent(userId, k -> new THashMap<>());
  }

  public void createOffer(Integer receiverId, Offer offer, String code) {
    getUserOffers(receiverId).put(code, offer);
  }

  public Offer getOfferByCode(int userId, String code) {
    Map<String, Offer> userOffers = getUserOffers(userId);
    return userOffers != null ? userOffers.get(code) : null;
  }

  public boolean isBeingOffered(int itemId) {
    return this.offers.values().stream()
        .anyMatch(o -> o.values().stream().anyMatch(offer -> offer.itemId() == itemId));
  }
}
