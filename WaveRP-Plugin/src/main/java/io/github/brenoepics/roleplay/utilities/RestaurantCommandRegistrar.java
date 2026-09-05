package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandHandler;
import io.github.brenoepics.roleplay.commands.jobs.restaurant.RestaurantCommand;
import io.github.brenoepics.roleplay.commands.jobs.restaurant.RestaurantCommand.Action;
import io.github.brenoepics.roleplay.commands.jobs.restaurant.RestaurantOrderResponseCommand;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.CheckDatabase;

public final class RestaurantCommandRegistrar {

  private RestaurantCommandRegistrar() {
  }

  public static void register() {
    registerCommand(new RestaurantCommand("cmd_restaurant_menu", new String[]{"menu"},
        Action.MENU, JobPermissions.RESTAURANT_MENU), new String[]{"menu"},
        ":menu - Affiche la carte du restaurant.");

    registerCommand(new RestaurantCommand("cmd_restaurant_take_order",
        new String[]{"prendrecommande"}, Action.TAKE_ORDER, JobPermissions.RESTAURANT_ORDER),
        new String[]{"prendrecommande"},
        ":prendrecommande <pseudo> - Propose de prendre la commande d'un client proche.");

    registerCommand(new RestaurantOrderResponseCommand("cmd_restaurant_accept_order",
        new String[]{"acceptercommande"}, true), new String[]{"acceptercommande"},
        ":acceptercommande - Accepte la proposition de prise de commande d'un restaurant.");

    registerCommand(new RestaurantOrderResponseCommand("cmd_restaurant_refuse_order",
        new String[]{"refusercommande"}, false), new String[]{"refusercommande"},
        ":refusercommande - Refuse la proposition de prise de commande d'un restaurant.");

    registerCommand(new RestaurantCommand("cmd_restaurant_prepare", new String[]{"preparer"},
        Action.PREPARE, JobPermissions.RESTAURANT_PREPARE), new String[]{"preparer"},
        ":preparer <plat> - Prépare un produit de la carte.");

    registerCommand(new RestaurantCommand("cmd_restaurant_serve", new String[]{"servir"},
        Action.SERVE, JobPermissions.RESTAURANT_SERVE), new String[]{"servir"},
        ":servir <pseudo> - Sert le plat préparé au client.");

    registerCommand(new RestaurantCommand("cmd_restaurant_bill", new String[]{"addition"},
        Action.BILL, JobPermissions.RESTAURANT_BILL), new String[]{"addition"},
        ":addition <pseudo> - Envoie l'addition au client.");

    registerCommand(new RestaurantCommand("cmd_restaurant_cash", new String[]{"encaisser"},
        Action.CASH, JobPermissions.RESTAURANT_CASH), new String[]{"encaisser"},
        ":encaisser <pseudo> - Encaisse l'addition et crédite la caisse du restaurant.");
  }

  private static void registerCommand(Command command, String[] keys, String description) {
    Emulator.getConfig().register("commands." + command.permission + ".keys", String.join(";", keys));
    Emulator.getTexts().register("commands.description." + command.permission, description);
    Emulator.getTexts().update("commands.description." + command.permission, description);
    CommandHandler.addCommand(command);
    CheckDatabase.registerPermission(command.permission, CheckDatabase.PermissionState.ALLOWED);
  }
}
