package com.eu.habbo.messages.outgoing.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandDocumentation;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.google.gson.JsonArray;
import com.google.gson.JsonObject;

import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
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
            item.addProperty("subcategory", details.subcategory);
            item.addProperty("access", details.access);
            commandItems.add(item);
            usedCategories.add(details.category);
        }

        JsonArray categories = new JsonArray();
        categories.add("Toutes");
        addCategory(categories, usedCategories, "RP");
        addCategory(categories, usedCategories, "Appartement");
        addCategory(categories, usedCategories, "Personnage");
        addCategory(categories, usedCategories, "Staff");
        addCategory(categories, usedCategories, "Général");

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

        usage = normalizeUsage(usage);
        CommandDocumentation.Metadata metadata = CommandDocumentation.resolve(command.permission, name);
        String descriptionOverride = CommandDocumentation.descriptionOverride(command.permission);
        if (descriptionOverride != null) {
            description = descriptionOverride;
        } else if (description.isEmpty() || description.equals("commands.description." + command.permission)) {
            description = metadata.defaultDescription;
        }

        List<String> aliases = new ArrayList<>();
        for (String key : keys) {
            String alias = ":" + key;
            if (!alias.equalsIgnoreCase(name) && !containsIgnoreCase(aliases, alias)) {
                aliases.add(alias);
            }
        }

        return new CommandDetails(name, aliases, description, usage, metadata.category,
                metadata.subcategory, metadata.access);
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

    private static String normalizeUsage(String usage) {
        return usage
                .replace("<username>", "[pseudo]")
                .replace("<user>", "[pseudo]")
                .replace("<pseudo>", "[pseudo]")
                .replace("<amount>", "[montant]")
                .replace("<montant>", "[montant]")
                .replace("<message>", "[message]")
                .replace("<text>", "[message]")
                .replace("<time in seconds>", "[durée en secondes]")
                .replace("<minutes>", "[minutes]")
                .replace("<raison>", "[raison]")
                .replace("<rank>", "[rang]")
                .replace("<metier|id>", "[métier|ID]")
                .replace("<code>", "[code]")
                .replace("<objet>", "[objet]");
    }

    private static class CommandDetails {
        private final String name;
        private final List<String> aliases;
        private final String description;
        private final String usage;
        private final String category;
        private final String subcategory;
        private final String access;

        private CommandDetails(String name, List<String> aliases, String description, String usage,
                               String category, String subcategory, String access) {
            this.name = name;
            this.aliases = aliases;
            this.description = description;
            this.usage = usage;
            this.category = category;
            this.subcategory = subcategory;
            this.access = access;
        }
    }
}
