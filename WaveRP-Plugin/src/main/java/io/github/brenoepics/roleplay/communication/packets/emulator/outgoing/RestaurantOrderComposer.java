package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;

/** Réutilise le popup d'offre existant pour demander au client d'accepter/refuser la prise de commande. */
public class RestaurantOrderComposer extends MessageComposer {

  private final String id;
  private final String restaurantName;
  private final Habbo employee;

  public RestaurantOrderComposer(Habbo employee, String restaurantName, String id) {
    this.employee = employee;
    this.restaurantName = restaurantName;
    this.id = id;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6002);
    this.response.appendString(this.id);
    this.response.appendString("Prise de commande - " + this.restaurantName);
    this.response.appendInt(0);
    this.response.appendString(this.employee.getHabboInfo().getUsername());
    this.response.appendString(this.employee.getHabboInfo().getLook());
    return this.response;
  }
}
