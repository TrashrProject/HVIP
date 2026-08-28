package io.github.brenoepics.roleplay.commands.macro;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.communication.outgoing.macro.OpenMacroComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;


public class OpenMacroCommand extends Command {

    public OpenMacroCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] strings) throws Exception {
         OpenMacroComposer openMacroComposer = new OpenMacroComposer();
        gameClient.sendResponse(new JavascriptCallbackComposer(openMacroComposer));
        return true;
    }
}