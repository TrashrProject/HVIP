package io.github.brenoepics.roleplay.features.farm.commands;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;

public class ReloadFarmCommand extends Command {

    public ReloadFarmCommand(String permission, String[] keys)
    {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params)
    {
        boolean loaded = RolePlay.getFarmManager().load();
        if(loaded)
            gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_update_farm.successfully"));
        else gameClient.getHabbo().whisper(Emulator.getTexts().getValue("commands.cmd_update_farm.error"));
        return true;
    }
}
