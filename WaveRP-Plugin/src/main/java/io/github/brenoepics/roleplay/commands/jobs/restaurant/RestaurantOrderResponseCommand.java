package io.github.brenoepics.roleplay.commands.jobs.restaurant;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.features.restaurant.RestaurantService;

public class RestaurantOrderResponseCommand extends Command {

  private final boolean accept;

  public RestaurantOrderResponseCommand(String permission, String[] keys, boolean accept) {
    super(permission, keys);
    this.accept = accept;
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (accept) {
      RestaurantService.acceptOrder(gameClient.getHabbo());
    } else {
      RestaurantService.refuseOrder(gameClient.getHabbo());
    }
    return true;
  }
}
