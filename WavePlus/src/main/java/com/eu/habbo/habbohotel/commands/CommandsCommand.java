package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.messages.outgoing.generic.alerts.MessagesForYouComposer;

import java.util.List;

public class CommandsCommand extends Command {
    public CommandsCommand() {
        super("cmd_commands", Emulator.getTexts().getValue("commands.keys.cmd_commands").split(";"));
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        List<Command> commands = Emulator.getGameEnvironment().getCommandHandler().getCommandsForRank(gameClient.getHabbo().getHabboInfo().getRank().getId());

        StringBuilder message = new StringBuilder(
                Emulator.getTexts().getValue("commands.generic.cmd_commands.text", "Vos commandes")
        );
        message.append(" (").append(commands.size()).append("):\r\n");

        for (Command command : commands) {
            message.append(Emulator.getTexts().getValue(
                    "commands.description." + command.permission,
                    "commands.description." + command.permission
            )).append("\r");
        }

        gameClient.sendResponse(new MessagesForYouComposer(new String[]{message.toString()}));

        return true;
    }
}
