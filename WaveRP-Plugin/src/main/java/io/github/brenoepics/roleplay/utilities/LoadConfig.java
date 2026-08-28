package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.Emulator;

public class LoadConfig {
    public static void ILoadConfig() {

            Emulator.getConfig().register("macro.configs.limit", "5");
            Emulator.getConfig().register("roleplay.rooms.change_clothing.enabled", "98");
            Emulator.getConfig().register("nahabbo.features.hospital.roomid", "0");
            Emulator.getConfig().register("features.hospital.autosend.seconds", "45");
            Emulator.getConfig().register("features.ems.call.cooldown.seconds", "30");
            Emulator.getConfig().register("features.ems.calls.list.limit", "15");
            Emulator.getConfig().register("features.ems.bandage.health", "20");
            Emulator.getConfig().register("features.ems.stabilize.seconds", "90");
            Emulator.getConfig().register("features.ems.revive.health", "35");
            Emulator.getConfig().register("features.ems.treatment.range", "1");
            Emulator.getConfig().register("features.ems.treatment.cooldown.seconds", "3");
            Emulator.getConfig().register("features.prison.release.roomid", "0");
            Emulator.getConfig().register("features.jailrock.time", "15");
            Emulator.getConfig().register("features.prison.look.m", "");
            Emulator.getConfig().register("features.prison.look.f", "");
            Emulator.getConfig().register("features.jailrock.effect", "30");
            Emulator.getConfig().register("features.healing.enableid", "23");
            Emulator.getConfig().register("nahabbo.features.jail.roomid", "0");
            Emulator.getConfig().register("nahabbo.features.death.mute", "29");
            Emulator.getConfig().register("nahabbo.features.trashbin.search.time", "1500");
            Emulator.getConfig().register("nahabbo.features.trashbin.cooldown", "300000");
            Emulator.getConfig().register("nahabbo.features.room.category", "10");
            Emulator.getConfig().register("features.safezone.category", "11");
            Emulator.getConfig().register("features.aggression.seconds", "600");
            Emulator.getConfig().register("features.start_work_timeout", "5");
            Emulator.getConfig().register("features.stop_work_timeout", "5");
            Emulator.getConfig().register("features.tazor_timeout", "5");
            Emulator.getConfig().register("features.offer_timeout", "5");
            Emulator.getConfig().register("features.arrest_timeout", "5");
            Emulator.getConfig().register("features.shoot_timeout", "3");
            Emulator.getConfig().register("features.rob_timeout", "3");
            Emulator.getConfig().register("features.hit_timeout", "3");
            Emulator.getConfig().register("features.apply_timeout", "3");
            Emulator.getConfig().register("features.help_timeout", "5");
            Emulator.getConfig().register("features.passive_timeout", "60");
            Emulator.getConfig().register("features.default.send_home.minutes", "5");
            Emulator.getConfig().register("features.send_home.min", "1");
            Emulator.getConfig().register("features.send_home.max", "15");
            Emulator.getConfig().register("features.police.tazor.effectid", "182");
            Emulator.getConfig().register("walking.rate.limit", "100");
            Emulator.getConfig().register("nahabbo.features.trashbin.items", "Bucks,Pizza,Medkit,Shield,Weapon,Nothing");
            Emulator.getConfig().register("nahabbo.features.trashbin.chances", "20,15,15,10,5,35");
            Emulator.getConfig().register("nahabbo.features.drugs.strength", "1.5");
            Emulator.getConfig().register("nahabbo.features.drugs.craft.time", "5");
            Emulator.getConfig().register("nahabbo.features.drugmachine.cooldown", "30");
            Emulator.getConfig().register("features.organizations.friendly_fire", "1");
            Emulator.getConfig().register("features.territory.claim.cooldown", "60");
            Emulator.getConfig().register("features.territory_war.tick.progress", "10");
            Emulator.getConfig().register("features.territory_war.ticks.milis", "2000");
            Emulator.getConfig().register("features.territory.defended.seconds_cooldown", "60");
            Emulator.getConfig().register("features.territory.claim.cooldown", "30");
            Emulator.getConfig().register("features.organizations.gang.price", "20");
            Emulator.getConfig().register("features.organizations.mafia.price", "20");
            Emulator.getConfig().register("features.organizations.cartel.price", "20");
            Emulator.getConfig().register("features.taxi.seconds_delay", "5");
            Emulator.getConfig().register("features.inventory.image.url", "https://example.com/inventory/%item%.png");
            Emulator.getConfig().register("features.payday.timer_minutes", "10");
            Emulator.getConfig().register("features.payday.rate", "3.0");
            Emulator.getConfig().register("features.police.tazor.duration_seconds", "10");
            Emulator.getConfig().register("features.wanted.minutes", "7");
            Emulator.getConfig().register("features.hunger.delay.minutes", "1");

            Emulator.getTexts().register("commands.cmd_update_farm.successfully", "La ferme a \u00e9t\u00e9 actualis\u00e9e.");
            Emulator.getTexts().register("commands.cmd_update_farm.error", "Impossible d'actualiser la ferme.");
            Emulator.getTexts().register("farm.warn.cooldown", "Vous devez attendre %time% seconde(s) avant de pouvoir r\u00e9colter de nouveau.");
            Emulator.getTexts().register("farm.warn.missing_effect", "Vous devez utiliser l'effet %effect% pour r\u00e9colter.");
            Emulator.getTexts().register("farm.warn.missing_item", "Vous avez besoin de %item% x%amount% pour r\u00e9colter.");
            Emulator.getTexts().register("farm.warn.harvested", "Vous avez r\u00e9colt\u00e9 %item%.");
            Emulator.getTexts().register("commands.keys.cmd_reload_farm", "update_farm;reloadfarm;farmreload");
            Emulator.getTexts().register("commands.description.cmd_reload_farm", ":actualiserferme");
            Emulator.getTexts().register("commands.cmd_sell_item.error.params", "Indiquez l'objet et la quantit\u00e9 \u00e0 vendre. Exemple : :vendreobjet carotte 10");
            Emulator.getTexts().register("commands.cmd_sell_item.error.params.item", "L'objet %item% est introuvable.");
            Emulator.getTexts().register("commands.cmd_sell_item.error.params.amount", "Indiquez une quantit\u00e9 valide.");
            Emulator.getTexts().register("commands.cmd_sell_item.successfully", "Vous avez vendu %amount% %item%.");
            Emulator.getTexts().register("commands.cmd_sell_item.error.wrong_room", "Vous ne pouvez pas vendre cet objet dans cette salle.");
            Emulator.getTexts().register("commands.cmd_update_item_marketplace.success", "Le march\u00e9 a \u00e9t\u00e9 actualis\u00e9.");

            Emulator.getConfig().register("farm.log_rewards", "1");

            Emulator.getTexts().register("commands.keys.cmd_sell_item", "vendreobjet;sell_item;sellitem;sell");
            Emulator.getTexts().register("commands.description.cmd_sell_item", ":vendreobjet <objet> <quantit\u00e9>");

            Emulator.getTexts().register("commands.keys.cmd_update_item_marketplace", "update_sellable;reloadsellable;update_sell");
            Emulator.getTexts().register("commands.description.cmd_update_item_marketplace", ":actualisermarche");



            Emulator.getTexts().register("roleplay.error.change_clothing.not_allowed", "Vous ne pouvez pas vous changer ici. Rendez-vous dans un magasin de v\u00eatements." );
    }
}
