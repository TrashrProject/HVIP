package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.incoming.macro.SelectMacroEvent.JSONSelectMacroEvent;
import io.github.brenoepics.roleplay.communication.outgoing.macro.MacroSetComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.macro.Macro;

public class SelectMacroEvent extends IncomingWebMessage<JSONSelectMacroEvent> {

  public SelectMacroEvent() {
    super(JSONSelectMacroEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONSelectMacroEvent message) {
    if (client.getHabbo() == null) {
      return;
    }

    Macro macro = RolePlay.getMacroManager()
        .getUserMacro(client.getHabbo().getHabboInfo().getId(), message.name);
    if (macro == null) {
      client.getHabbo().alert(Emulator.getTexts().getValue("macro.not-found",
          "Sorry, I couldn't find this macro config, try to create some!"));
      return;
    }

    client.getHabbo().getHabboStats().cache.put("macro", macro.getId());
    MacroSetComposer macrosetcomposer = new MacroSetComposer(macro);
    client.sendResponse(new JavascriptCallbackComposer(macrosetcomposer));
  }

  static class JSONSelectMacroEvent {
    String name;
  }
}