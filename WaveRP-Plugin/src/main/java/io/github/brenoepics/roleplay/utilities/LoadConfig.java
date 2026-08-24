package io.github.brenoepics.roleplay.utilities;

import com.eu.habbo.Emulator;

public class LoadConfig {
    public static void ILoadConfig() {

            Emulator.getConfig().register("macro.configs.limit", "5");
            Emulator.getConfig().register("roleplay.rooms.change_clothing.enabled", "1;2;3");
            Emulator.getConfig().register("nahabbo.features.hospital.roomid", "0");
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

            Emulator.getTexts().register("commands.cmd_update_farm.successfully", "Successfully updated the farm!");
            Emulator.getTexts().register("commands.cmd_update_farm.error", "Error updating the farm!");
            Emulator.getTexts().register("farm.warn.cooldown", "You need to wait %time% seconds before you can farm again!");
            Emulator.getTexts().register("farm.warn.missing_effect", "You need the effect %effect% to farm!");
            Emulator.getTexts().register("farm.warn.missing_item", "You need the item %item% x%amount% to farm!");
            Emulator.getTexts().register("farm.warn.harvested", "You have successfully harvested %item%!");
            Emulator.getTexts().register("commands.keys.cmd_reload_farm", "update_farm;reloadfarm;farmreload");
            Emulator.getTexts().register("commands.description.cmd_reload_farm", ":update_farm");
            Emulator.getTexts().register("commands.cmd_sell_item.error.params", "You need to specify the item name and the amount to sell! Example: :sellitem carrot 10");
            Emulator.getTexts().register("commands.cmd_sell_item.error.params.item", "Could not find the item %item%!");
            Emulator.getTexts().register("commands.cmd_sell_item.error.params.amount", "You need to specify a valid amount!");
            Emulator.getTexts().register("commands.cmd_sell_item.successfully", "Successfully sold %amount% %item%!");
            Emulator.getTexts().register("commands.cmd_sell_item.error.wrong_room", "Oops! You cannot sell this item in this room!");
            Emulator.getTexts().register("commands.cmd_update_item_marketplace.success", "Successfully updated the marketplace!");

            Emulator.getConfig().register("farm.log_rewards", "1");

            Emulator.getTexts().register("commands.keys.cmd_sell_item", "sell_item;sellitem;sell");
            Emulator.getTexts().register("commands.description.cmd_sell_item", ":sellitem <item> <amount>");

            Emulator.getTexts().register("commands.keys.cmd_update_item_marketplace", "update_sellable;reloadsellable;update_sell");
            Emulator.getTexts().register("commands.description.cmd_update_item_marketplace", ":update_sellable");



            Emulator.getTexts().register("roleplay.error.change_clothing.not_allowed", "Sorry, you can't change in here, visit a clothing store!" );
    }
}
