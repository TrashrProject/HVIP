package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.incoming.macro.CreateMacroEvent.JSONCreateMacroEvent;
import io.github.brenoepics.roleplay.communication.outgoing.macro.MacroSetComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.macro.Macro;
import io.github.brenoepics.roleplay.features.macro.MacroManager;

public class CreateMacroEvent extends IncomingWebMessage<JSONCreateMacroEvent> {

  public CreateMacroEvent() {
    super(JSONCreateMacroEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONCreateMacroEvent message) {
    MacroManager macroManager = RolePlay.getMacroManager();
    if (macroManager.habboHasMacro(client.getHabbo().getHabboInfo().getId(), message.name)) {
      client.getHabbo().alert(Emulator.getTexts()
          .getValue("macro.duplicate", "Vous possédez déjà une macro portant ce nom."));
      return;
    }
    if (message.name.length() < 4 || message.name.length() > 15) {
      client.getHabbo().alert(Emulator.getTexts()
          .getValue("macro.size", "Le nom de votre macro doit contenir entre 4 et 15 caractères."));
      return;
    }
    if (macroManager.getUserMacros(client.getHabbo().getHabboInfo().getId()).size()
        >= Emulator.getConfig().getInt("macro.configs.limit", 5)) {
      client.getHabbo().alert(Emulator.getTexts()
          .getValue("macro.limit", "Vous avez trop de macros. Supprimez-en avant d'en créer une nouvelle."));
      return;
    }

    if (!macroManager.createMacro(client.getHabbo().getHabboInfo().getId(), message.name)) {
      client.getHabbo().alert(Emulator.getTexts()
          .getValue("macro.create.failed", "Failed to create macro. Please try again later."));
      return;
    }

    Macro macro = macroManager.getUserMacro(client.getHabbo().getHabboInfo().getId(), message.name);
    if (macro == null) {
      return;
    }
    client.getHabbo().getHabboStats().cache.put("macro", macro.getId());
    MacroSetComposer macrosetcomposer = new MacroSetComposer(
        macroManager.getUserMacro(client.getHabbo().getHabboInfo().getId(), message.name));
    client.sendResponse(new JavascriptCallbackComposer(macrosetcomposer));
  }

  static class JSONCreateMacroEvent {

    String name;
  }
}
