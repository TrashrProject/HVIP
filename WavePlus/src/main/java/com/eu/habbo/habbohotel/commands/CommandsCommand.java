package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.messages.outgoing.generic.alerts.GenericAlertComposer;
import com.eu.habbo.messages.outgoing.generic.CommandsWindowComposer;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;

public class CommandsCommand extends Command {
    public CommandsCommand() {
        super("cmd_commands", Emulator.getTexts().getValue("commands.keys.cmd_commands").split(";"));
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        List<Command> rankCommands = Emulator.getGameEnvironment()
                .getCommandHandler()
                .getCommandsForRank(gameClient.getHabbo().getHabboInfo().getRank().getId());

        List<Command> commands = new ArrayList<>();

        for (Command command : rankCommands) {
            if (getPrimaryKey(command) != null) {
                commands.add(command);
            }
        }

        Collections.sort(commands, new Comparator<Command>() {
            @Override
            public int compare(Command first, Command second) {
                return String.CASE_INSENSITIVE_ORDER.compare(
                        getPrimaryKey(first),
                        getPrimaryKey(second)
                );
            }
        });

        for (CommandsWindowComposer response : CommandsWindowComposer.createResponses(commands)) {
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
