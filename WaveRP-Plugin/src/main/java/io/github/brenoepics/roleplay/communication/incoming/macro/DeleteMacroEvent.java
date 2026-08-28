package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.features.macro.Macro;
import io.github.brenoepics.roleplay.features.macro.MacroManager;

public class DeleteMacroEvent extends IncomingWebMessage<DeleteMacroEvent.JSONDeleteMacroEvent> {

  public DeleteMacroEvent() {
    super(JSONDeleteMacroEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONDeleteMacroEvent message) {
    if (client.getHabbo() == null) {
      return;
    }

    MacroManager macroManager = RolePlay.getMacroManager();
    Macro macro = macroManager.getUserMacro(client.getHabbo().getHabboInfo().getId(), message.name);
    if (macro == null) {
      client.getHabbo().alert(Emulator.getTexts().getValue("macro.not-found",
          "Sorry, I couldn't find this macro config, try to create some!"));
      return;
    }

    macroManager.deleteMacro(client.getHabbo().getHabboInfo().getId(), macro.getId());
  }

  static class JSONDeleteMacroEvent {

    String name;
  }
}