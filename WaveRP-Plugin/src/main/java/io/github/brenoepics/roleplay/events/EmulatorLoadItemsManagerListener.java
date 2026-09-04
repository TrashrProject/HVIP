package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.items.ItemInteraction;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.emulator.EmulatorLoadItemsManagerEvent;
import io.github.brenoepics.roleplay.features.items.interactions.ClaimTerritoryItem;
import io.github.brenoepics.roleplay.features.items.interactions.DrugMachine;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionATM;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionBankComputer;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionCoffeeMachine;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionFarmable;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionJobGate;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPBed;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPRock;
import io.github.brenoepics.roleplay.features.items.interactions.InteractionRPTeleportTile;
import io.github.brenoepics.roleplay.features.items.interactions.TrashBin;
import io.github.brenoepics.roleplay.features.items.interactions.WardrobeCabin;

public class EmulatorLoadItemsManagerListener implements EventListener {

  @EventHandler
  public static void onLoadItemsManager(EmulatorLoadItemsManagerEvent e) {
    addInteraction("rpteleporttile", InteractionRPTeleportTile.class);
    addInteraction("nahabbo_rp_trashbin", TrashBin.class);
    addInteraction("rp_drugmachine", DrugMachine.class);
    addInteraction("rp_claim_territory", ClaimTerritoryItem.class);
    addInteraction("wardrobe_cabin", WardrobeCabin.class);
    addInteraction("rp_bed", InteractionRPBed.class);
    addInteraction("rp_rock", InteractionRPRock.class);
    addInteraction("rp_jobgate", InteractionJobGate.class);
    addInteraction("farmable", InteractionFarmable.class);
    addInteraction("rp_atm", InteractionATM.class);
    addInteraction("rp_bank_computer", InteractionBankComputer.class);
    addInteraction("rp_coffee_machine", InteractionCoffeeMachine.class);
  }

  public static void addInteraction(String name, Class<? extends HabboItem> clazz) {
    Emulator.getGameEnvironment().getItemManager()
        .addItemInteraction(new ItemInteraction(name, clazz));
  }
}
