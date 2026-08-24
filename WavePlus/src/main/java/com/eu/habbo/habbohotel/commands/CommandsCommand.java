package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.messages.outgoing.generic.CommandsWindowComposer;

import java.util.List;

public class CommandsCommand extends Command {
    public CommandsCommand() {
        super("cmd_commands", Emulator.getTexts().getValue("commands.keys.cmd_commands").split(";"));
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) throws Exception {
        List<Command> commands = Emulator.getGameEnvironment().getCommandHandler().getCommandsForRank(gameClient.getHabbo().getHabboInfo().getRank().getId());
        gameClient.sendResponse(new CommandsWindowComposer(commands));

        return true;
    }
}
