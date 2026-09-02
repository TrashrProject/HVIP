package com.eu.habbo.messages.outgoing.generic;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.commands.CommandViewRegistry;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.eu.habbo.messages.outgoing.Outgoing;
import com.google.gson.JsonArray;
import com.google.gson.JsonObject;

import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.Base64;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * Opens the native WaveRP command centre already bundled in the Nitro client.
 * The payload is sent through LinkEvent (packet 2023), matching the historical
 * WaveRP implementation used by the commands UI.
 */
public class CommandsWindowComposer extends MessageComposer {
    private static final Logger LOGGER = LoggerFactory.getLogger(CommandsWindowComposer.class);
    private static final int CHUNK_SIZE = 30000;

    private final String linkEvent;

    private CommandsWindowComposer(String linkEvent) {
        this.linkEvent = linkEvent;
    }

    public static List<CommandsWindowComposer> createResponses(List<Command> commands, GameClient client) {
        String payload = buildPayload(commands, client).toString();
        String encoded = Base64.getUrlEncoder().withoutPadding()
                .encodeToString(payload.getBytes(StandardCharsets.UTF_8));
        int total = (encoded.length() + CHUNK_SIZE - 1) / CHUNK_SIZE;
        String transferId = Long.toHexString(System.nanoTime());
        List<CommandsWindowComposer> responses = new ArrayList<>(total);

        for (int index = 0; index < total; index++) {
            int start = index * CHUNK_SIZE;
            int end = Math.min(start + CHUNK_SIZE, encoded.length());
            String event = "commands/chunk/" + transferId + "/" + index + "/" + total + "/"
                    + encoded.substring(start, end);
            responses.add(new CommandsWindowComposer(event));
        }

        return responses;
    }

    @Override
    protected ServerMessage composeInternal() {
        this.response.init(Outgoing.NuxAlertComposer);
        this.response.appendString(this.linkEvent);
        return this.response;
    }

    private static JsonObject buildPayload(List<Command> commands, GameClient client) {
        JsonObject payload = new JsonObject();
        JsonObject data = new JsonObject();
        JsonArray commandItems = new JsonArray();
        Set<String> usedCategories = new LinkedHashSet<>();

        payload.addProperty("header", "commands");

        for (Command command : commands) {
            CommandDetails details;
            try {
                details = getDetails(command, client);
            } catch (RuntimeException exception) {
                LOGGER.error("Unable to serialize command {} ({}) for :commands",
                        command == null ? "<null>" : command.getClass().getName(),
                        command == null ? "no permission" : command.permission, exception);
                continue;
            }
            if (details == null) {
                LOGGER.warn("Skipping invalid command entry {} ({}) in :commands",
                        command == null ? "<null>" : command.getClass().getName(),
                        command == null ? "no permission" : command.permission);
                continue;
            }

            JsonObject item = new JsonObject();
            JsonArray aliases = new JsonArray();
            for (String alias : details.aliases) aliases.add(alias);

            item.addProperty("name", details.name);
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
        addCategory(categories, usedCategories, "Général");
        addCategory(categories, usedCategories, "RP");
        addCategory(categories, usedCategories, "Économie");
        addCategory(categories, usedCategories, "Social");
        addCategory(categories, usedCategories, "Déplacements");
        addCategory(categories, usedCategories, "Utilitaires");
        addCategory(categories, usedCategories, "Métiers");
        addCategory(categories, usedCategories, "Staff");
        addCategory(categories, usedCategories, "Administration");
        for (String category : usedCategories) addCategory(categories, usedCategories, category);

        data.add("commands", commandItems);
        data.add("categories", categories);
        payload.add("data", data);
        return payload;
    }

    private static void addCategory(JsonArray categories, Set<String> usedCategories, String category) {
        if (!usedCategories.contains(category)) return;
        for (int index = 0; index < categories.size(); index++) {
            if (category.equals(categories.get(index).getAsString())) return;
        }
        categories.add(category);
    }

    private static CommandDetails getDetails(Command command, GameClient client) {
        if (command == null || command.keys == null) return null;

        List<String> keys = new ArrayList<>();
        for (String key : command.keys) {
            if (key != null && !key.trim().isEmpty() && !containsIgnoreCase(keys, key.trim())) {
                keys.add(key.trim());
            }
        }
        if (keys.isEmpty()) return null;

        String name = ":" + keys.get(0);
        String permission = command.permission == null ? "" : command.permission;
        String translationKey = "commands.description." + permission;
        String localized = Emulator.getTexts().getValue(translationKey, "").trim();

        String usage = name;
        String description = "Utilisez cette commande dans le chat de l'appartement.";

        if (!localized.isEmpty() && !localized.equalsIgnoreCase(translationKey)) {
            String firstLine = firstLine(localized);
            if (firstLine.startsWith(":")) {
                usage = firstLine;
                int separator = firstLine.indexOf(" - ");
                if (separator > 0) {
                    usage = firstLine.substring(0, separator).trim();
                    description = firstLine.substring(separator + 3).trim();
                }
            } else {
                description = firstLine;
            }
        }

        List<String> aliases = new ArrayList<>();
        for (int i = 1; i < keys.size(); i++) aliases.add(":" + keys.get(i));

        Category category = categoryFor(permission, keys.get(0));
        String customCategory = CommandViewRegistry.category(client, command);
        String customSubcategory = CommandViewRegistry.subcategory(client, command);
        String customAccess = CommandViewRegistry.access(client, command);
        if (customCategory != null) category = new Category(customCategory,
                customSubcategory == null ? customCategory : customSubcategory,
                customAccess == null ? category.access : customAccess);
        return new CommandDetails(name, aliases, description, normalizeUsage(usage),
                category.category, category.subcategory, category.access);
    }

    private static Category categoryFor(String permission, String key) {
        String value = (permission + " " + key).toLowerCase(Locale.ROOT);

        if (containsAny(value, "bank", "solde", "virement", "deposer", "depot", "deposit", "retirer", "withdraw", "historique", "transactions", "openaccount", "ouvrircompte", "versement", "retraitclient", "fermercompte")) {
            boolean employee = permission.startsWith("cmd_bank_") || permission.equals("cmd_openaccount");
            return new Category(employee ? "Métiers" : "Économie", "Banque",
                    employee ? "Banque en service · grade autorisé · ordinateur connecté" : "Compte bancaire actif");
        }

        if (containsAny(value, "911", "police", "tazor", "taser", "handcuff", "escort", "prison", "release", "arrest", "wanted", "charge", "pardon", "detaser")) {
            return new Category("Métiers", "Police", "Policier en service au lieu de travail · grade autorisé");
        }

        if (containsAny(value, "ems", "medical", "medecin", "ambulance", "heal", "bandage", "stabil", "revive", "patient", "diagnostic")) {
            return new Category("Métiers", "Hôpital", "Personnel médical en service au lieu de travail · grade autorisé");
        }

        if (containsAny(value, "taxi")) {
            return new Category("Métiers", "Taxi", "Chauffeur en service · grade autorisé");
        }

        if (containsAny(value, "job", "work", "hire", "fire", "promote", "demote", "apply", "sell_rpitem", "offer_rpitem", "accept_offer", "decline_offer", "send_home")) {
            return new Category("Métiers", "Gestion et service", "Métier en service au lieu de travail · grade autorisé");
        }

        if (containsAny(value, "business", "cashregister", "inventory", "security")) {
            return new Category("Métiers", "Entreprise", "Employé en service au lieu de travail · grade autorisé");
        }

        if (containsAny(value, "bucks", "rob", "shoot", "hit", "spit", "equip", "unequip", "passive", "combat", "org_", "rpitem")) {
            return new Category("RP", "Roleplay", "Accessible selon votre situation RP");
        }

        if (containsAny(value, "ban", "mute", "alert", "staff", "shutdown", "update_", "super", "give_rank", "mass", "userinfo", "invisible", "summon", "stalk")) {
            return new Category("Staff", "Staff", "R\u00e9serv\u00e9 au staff");
        }

        return new Category("Général", "Général", "Accessible à votre rang");
    }

    private static boolean containsAny(String value, String... needles) {
        for (String needle : needles) if (value.contains(needle)) return true;
        return false;
    }

    private static String firstLine(String value) {
        int lineBreak = value.indexOf('\n');
        return (lineBreak >= 0 ? value.substring(0, lineBreak) : value).replace("\r", "").trim();
    }

    private static boolean containsIgnoreCase(List<String> values, String expected) {
        for (String value : values) if (value.equalsIgnoreCase(expected)) return true;
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
                .replace("<minutes>", "[minutes]")
                .replace("<code>", "[code]");
    }

    private static class Category {
        private final String category;
        private final String subcategory;
        private final String access;

        private Category(String category, String subcategory, String access) {
            this.category = category;
            this.subcategory = subcategory;
            this.access = access;
        }
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
