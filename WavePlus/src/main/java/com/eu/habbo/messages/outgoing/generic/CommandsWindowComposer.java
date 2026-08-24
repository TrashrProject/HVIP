package com.eu.habbo.messages.outgoing.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.google.gson.JsonArray;
import com.google.gson.JsonObject;

import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;

public class CommandsWindowComposer extends MessageComposer {
    private static final int LINK_EVENT_PACKET_ID = 2023;

    private final List<Command> commands;

    public CommandsWindowComposer(List<Command> commands) {
        this.commands = commands;
    }

    @Override
    protected ServerMessage composeInternal() {
        JsonObject payload = new JsonObject();
        JsonObject data = new JsonObject();
        JsonArray commandItems = new JsonArray();
        Set<String> usedCategories = new LinkedHashSet<>();

        payload.addProperty("header", "commands");

        for (Command command : this.commands) {
            CommandDetails details = getDetails(command);
            JsonObject item = new JsonObject();
            JsonArray aliases = new JsonArray();

            item.addProperty("name", details.name);
            for (String alias : details.aliases) {
                aliases.add(alias);
            }
            item.add("aliases", aliases);
            item.addProperty("description", details.description);
            item.addProperty("usage", details.usage);
            item.addProperty("category", details.category);
            commandItems.add(item);
            usedCategories.add(details.category);
        }

        JsonArray categories = new JsonArray();
        categories.add("All");
        addCategory(categories, usedCategories, "Roleplay");
        addCategory(categories, usedCategories, "Appartement");
        addCategory(categories, usedCategories, "Personnage");
        addCategory(categories, usedCategories, "Staff");
        addCategory(categories, usedCategories, "General");

        data.add("commands", commandItems);
        data.add("categories", categories);
        payload.add("data", data);

        this.response.init(LINK_EVENT_PACKET_ID);
        this.response.appendString("habblet/open/" + payload.toString().replace("/", "&#47;"));
        return this.response;
    }

    private static void addCategory(JsonArray categories, Set<String> usedCategories, String category) {
        if (usedCategories.contains(category)) {
            categories.add(category);
        }
    }

    private static CommandDetails getDetails(Command command) {
        List<String> keys = new ArrayList<>();
        if (command.keys != null) {
            for (String key : command.keys) {
                if (key != null && !key.trim().isEmpty() && !containsIgnoreCase(keys, key.trim())) {
                    keys.add(key.trim());
                }
            }
        }

        String fallbackKey = command.permission == null ? "commande" : command.permission.replaceFirst("^cmd_", "");
        String name = ":" + (keys.isEmpty() ? fallbackKey : keys.get(0));
        String localized = Emulator.getTexts().getValue(
                "commands.description." + command.permission,
                name
        ).trim();
        String usage = localized.startsWith(":") ? firstLine(localized) : name;
        String description = localized.startsWith(":") ? "" : localized;

        int separator = usage.indexOf(" - ");
        if (separator > 0) {
            description = usage.substring(separator + 3).trim();
            usage = usage.substring(0, separator).trim();
        }

        String usageCommand = usage.split("\\s+", 2)[0];
        if (usageCommand.startsWith(":")) {
            name = usageCommand;
        }

        String category = getCategory(command.permission, name);
        if (description.isEmpty() || description.equals("commands.description." + command.permission)) {
            description = getDefaultDescription(category);
        }

        List<String> aliases = new ArrayList<>();
        for (String key : keys) {
            String alias = ":" + key;
            if (!alias.equalsIgnoreCase(name) && !containsIgnoreCase(aliases, alias)) {
                aliases.add(alias);
            }
        }

        return new CommandDetails(name, aliases, description, usage, category);
    }

    private static String firstLine(String value) {
        int lineBreak = value.indexOf('\n');
        return (lineBreak >= 0 ? value.substring(0, lineBreak) : value).replace("\r", "").trim();
    }

    private static boolean containsIgnoreCase(List<String> values, String expected) {
        for (String value : values) {
            if (value.equalsIgnoreCase(expected)) {
                return true;
            }
        }
        return false;
    }

    private static String getCategory(String permission, String name) {
        String value = ((permission == null ? "" : permission) + " " + name).toLowerCase(Locale.ROOT);

        if (value.contains("superhire")) {
            return "Staff";
        }

        if (containsAny(value, "job", "work", "taxi", "rob", "hit", "shoot", "equip", "passive",
                "heal", "tazor", "tase", "arrest", "release", "pardon", "charge", "hire", "apply",
                "promote", "demote", "org_", "organization", "territory", "wanted", "combat", "target",
                "account", "balance", "deposit", "withdraw", "transaction", "offer", "sell_rpitem", "911")) {
            return "Roleplay";
        }

        if (containsAny(value, "room", "eject", "pickall", "setmax", "setspeed", "diagonal", "coords",
                "teleport", "bundle", "buildheight", "closedice", "setpublic", "buyroom", "sellroom")) {
            return "Appartement";
        }

        if (containsAny(value, "ban", "mute", "kick", "alert", "rank", "staff", "shutdown", "update_",
                "reload", "mass", "disconnect", "summon", "control", "userinfo", "freeze", "gift",
                "points", "credits", "duckets", "badge", "plugins", "filter", "unload")) {
            return "Staff";
        }

        if (containsAny(value, "mimic", "moonwalk", "fastwalk", "sit", "lay", "dance", "effect", "faceless",
                "changename", "hand_item", "kiss", "hug", "brb", "hoverboard")) {
            return "Personnage";
        }

        return "General";
    }

    private static boolean containsAny(String value, String... fragments) {
        for (String fragment : fragments) {
            if (value.contains(fragment)) {
                return true;
            }
        }
        return false;
    }

    private static String getDefaultDescription(String category) {
        switch (category) {
            case "Roleplay":
                return "Commande liee aux actions roleplay.";
            case "Appartement":
                return "Commande de gestion d'appartement.";
            case "Personnage":
                return "Commande liee a votre personnage.";
            case "Staff":
                return "Commande de moderation ou d'administration.";
            default:
                return "Commande generale.";
        }
    }

    private static class CommandDetails {
        private final String name;
        private final List<String> aliases;
        private final String description;
        private final String usage;
        private final String category;

        private CommandDetails(String name, List<String> aliases, String description, String usage, String category) {
            this.name = name;
            this.aliases = aliases;
            this.description = description;
            this.usage = usage;
            this.category = category;
        }
    }
}
