package io.github.brenoepics.roleplay.commands.jobs.restaurant;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.features.restaurant.RestaurantService;
import java.util.Arrays;

public class RestaurantCommand extends Command {

  public enum Action {
    MENU,
    TAKE_ORDER,
    PREPARE,
    SERVE,
    BILL,
    CASH
  }

  private final Action action;
  private final String jobPermission;

  public RestaurantCommand(String permission, String[] keys, Action action, String jobPermission) {
    super(permission, keys);
    this.action = action;
    this.jobPermission = jobPermission;
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo employee = gameClient.getHabbo();
    if (!RestaurantService.authorize(employee, jobPermission)) {
      return true;
    }

    switch (action) {
      case MENU -> RestaurantService.showMenu(employee);
      case TAKE_ORDER -> {
        if (params.length < 2) {
          employee.whisper("Usage : :prendrecommande <pseudo>");
          return true;
        }
        RestaurantService.takeOrder(employee, params[1]);
      }
      case PREPARE -> {
        if (params.length < 2) {
          employee.whisper("Usage : :preparer <plat>");
          return true;
        }
        RestaurantService.prepare(employee,
            String.join(" ", Arrays.copyOfRange(params, 1, params.length)));
      }
      case SERVE -> {
        if (params.length < 2) {
          employee.whisper("Usage : :servir <pseudo>");
          return true;
        }
        RestaurantService.serve(employee, params[1]);
      }
      case BILL -> {
        if (params.length < 2) {
          employee.whisper("Usage : :addition <pseudo>");
          return true;
        }
        RestaurantService.sendBill(employee, params[1]);
      }
      case CASH -> {
        if (params.length < 2) {
          employee.whisper("Usage : :encaisser <pseudo>");
          return true;
        }
        RestaurantService.cash(employee, params[1]);
      }
    }
    return true;
  }
}
