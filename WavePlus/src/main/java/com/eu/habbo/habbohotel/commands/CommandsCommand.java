package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.messages.outgoing.generic.CommandsWindowComposer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.LinkedHashMap;
import java.util.Locale;
import java.util.Map;

public class CommandsCommand extends Command {
    private static final Logger LOGGER = LoggerFactory.getLogger(CommandsCommand.class);

    public CommandsCommand() {
        super("cmd_commands", Emulator.getTexts().getValue("commands.keys.cmd_commands").split(";"));
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        List<Command> rankCommands = Emulator.getGameEnvironment()
                .getCommandHandler()
                .getCommandsForRank(gameClient.getHabbo().getHabboInfo().getRank().getId());

        Map<String, Command> uniqueCommands = new LinkedHashMap<>();

        for (Command command : rankCommands) {
            String primaryKey = getPrimaryKey(command);
            if (primaryKey != null && CommandViewRegistry.isVisible(gameClient, command)) {
                uniqueCommands.putIfAbsent(primaryKey.toLowerCase(Locale.ROOT), command);
            }
        }

        List<Command> commands = new ArrayList<>(uniqueCommands.values());

        Collections.sort(commands, new Comparator<Command>() {
            @Override
            public int compare(Command first, Command second) {
                return String.CASE_INSENSITIVE_ORDER.compare(
                        getPrimaryKey(first),
                        getPrimaryKey(second)
                );
            }
        });

        List<CommandsWindowComposer> responses = CommandsWindowComposer.createResponses(commands, gameClient);
        LOGGER.info(":commands requested by {} (rank {}, {} commands, {} packets)",
                gameClient.getHabbo().getHabboInfo().getUsername(),
                gameClient.getHabbo().getHabboInfo().getRank().getId(), commands.size(), responses.size());
        for (CommandsWindowComposer response : responses) {
            gameClient.sendResponse(response);
        }

        return true;
    }

    private static String getPrimaryKey(Command command) {
        if (command == null || command.keys == null) {
            return null;
        }

        for (String key : command.keys) {
            if (key != null && !key.trim().isEmpty()) {
                return key.trim();
            }
        }

        return null;
    }

    private static String getDescription(Command command) {
        if (command == null || command.permission == null || command.permission.trim().isEmpty()) {
            return null;
        }

        String translationKey = "commands.description." + command.permission;
        String description = Emulator.getTexts().getValue(translationKey, "");

        if (description == null) {
            return null;
        }

        description = description
                .replace('\r', ' ')
                .replace('\n', ' ')
                .trim();

        while (description.contains("  ")) {
            description = description.replace("  ", " ");
        }

        if (description.isEmpty() || description.equalsIgnoreCase(translationKey)) {
            return null;
        }

        return description;
    }
}
