package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.gameclients.GameClient;

/** Optional presentation/visibility bridge used by plugins without coupling the core to them. */
public interface CommandViewProvider {
    boolean isVisible(GameClient client, Command command);
    String category(GameClient client, Command command);
    String subcategory(GameClient client, Command command);
    String access(GameClient client, Command command);
}
