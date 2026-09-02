package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;

public final class CommandViewRegistry {
    private static volatile CommandViewProvider provider;

    private CommandViewRegistry() {}

    public static void setProvider(CommandViewProvider value) {
        provider = value;
    }

    public static boolean isVisible(GameClient client, Command command) {
        return provider == null || provider.isVisible(client, command);
    }

    public static String category(GameClient client, Command command) {
        return provider == null ? null : provider.category(client, command);
    }

    public static String subcategory(GameClient client, Command command) {
        return provider == null ? null : provider.subcategory(client, command);
    }

    public static String access(GameClient client, Command command) {
        return provider == null ? null : provider.access(client, command);
    }
}
