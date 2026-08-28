package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.outgoing.macro.MacroSetComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.macro.Macro;
import io.github.brenoepics.roleplay.features.macro.MacroManager;
import org.json.JSONObject;

public class MacroSaveEvent extends IncomingWebMessage<MacroSaveEvent.JSONMacroSaveEvent> {

  public MacroSaveEvent() {
    super(JSONMacroSaveEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONMacroSaveEvent message) {
    MacroManager macroManager = RolePlay.getMacroManager();
    if (!macroManager.habboHasMacro(client.getHabbo().getHabboInfo().getId(), message.name)) {
      return;
    }

    Macro macro = macroManager.getUserMacro(client.getHabbo().getHabboInfo().getId(),
        message.name);

    if (macro == null) {
      client.getHabbo().alert(Emulator.getTexts().getValue("macro.not-found",
          "Sorry, I couldn't find this macro config, try to create some!"));
      return;
    }

    if (macroManager.editMacro(client.getHabbo().getHabboInfo().getId(), macro.getId(),
        new JSONObject(message.macros))) {
      MacroSetComposer macrosetcomposer = new MacroSetComposer(
          macroManager.getUserMacro(client.getHabbo().getHabboInfo().getId(), macro.getName()));
      client.sendResponse(new JavascriptCallbackComposer(macrosetcomposer));
    }
  }

  static class JSONMacroSaveEvent {

    String name;
    String macros;
  }
}