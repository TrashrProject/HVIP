package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.utilities.types.RPItem;

public class OfferComposer extends MessageComposer {

  private final String id;
  private final RPItem item;
  private final Habbo offering;

  public OfferComposer(Habbo offering, RPItem item, String offerId) {
    this.offering = offering;
    this.item = item;
    this.id = offerId;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6002);

    this.response.appendString(this.id);
    this.response.appendString(this.item.getDisplayName());
    this.response.appendInt(this.item.getPrice());
    this.response.appendString(this.offering.getHabboInfo().getUsername());
    this.response.appendString(this.offering.getHabboInfo().getLook());

    return this.response;
  }
}
