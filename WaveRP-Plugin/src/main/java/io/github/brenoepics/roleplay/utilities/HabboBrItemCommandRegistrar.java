package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandHandler;
import io.github.brenoepics.roleplay.commands.combat.ReloadCommand;
import io.github.brenoepics.roleplay.commands.generic.FishingCommand;
import io.github.brenoepics.roleplay.commands.generic.PlantCommand;

/** Registers the ParadiseRP adaptations of the useful HabboRPbr item actions. */
public final class HabboBrItemCommandRegistrar {

  private HabboBrItemCommandRegistrar() {
  }

  public static void register() {
    registerCommand(
        new ReloadCommand("cmd_reload_weapon", new String[]{"recharger", "reload"}),
        new String[]{"recharger", "reload"},
        ":recharger - Recharge l'arme à feu équipée avec les Munitions de l'inventaire.");

    registerCommand(
        new FishingCommand("cmd_fishing", new String[]{"pecher", "pêcher", "fish"}),
        new String[]{"pecher", "pêcher", "fish"},
        ":pecher - Utilise une Canne à pêche pour attraper du poisson.");

    registerCommand(
        new PlantCommand("cmd_plant_seed",
            new String[]{"planter", "recolter", "récolter", "harvest"}),
        new String[]{"planter", "recolter", "récolter", "harvest"},
        ":planter - Plante une Graine. :recolter - Récupère la culture arrivée à maturité.");
  }

  private static void registerCommand(Command command, String[] keys, String description) {
    Emulator.getConfig().register("commands." + command.permission + ".keys", String.join(";", keys));
    Emulator.getTexts().register("commands.description." + command.permission, description);
    Emulator.getTexts().update("commands.description." + command.permission, description);
    CommandHandler.addCommand(command);
    CheckDatabase.registerPermission(command.permission, CheckDatabase.PermissionState.ALLOWED);
  }
}
