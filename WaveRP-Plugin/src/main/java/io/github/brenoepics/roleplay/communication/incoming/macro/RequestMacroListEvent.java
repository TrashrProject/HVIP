package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.incoming.macro.RequestMacroListEvent.JSONRequestMacroListEvent;
import io.github.brenoepics.roleplay.communication.outgoing.macro.MacroListComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.macro.Macro;
import java.util.Map;

public class RequestMacroListEvent extends IncomingWebMessage<JSONRequestMacroListEvent> {

  public RequestMacroListEvent() {
    super(JSONRequestMacroListEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONRequestMacroListEvent message) {
      if (client.getHabbo() == null) {
          return;
      }

    Map<Integer, Macro> macros = RolePlay.getMacroManager()
        .getUserMacros(client.getHabbo().getHabboInfo().getId());
    if (macros == null) {
      return;
    }

    MacroListComposer macrolistcomposer = new MacroListComposer(macros.values());
    client.sendResponse(new JavascriptCallbackComposer(macrolistcomposer));
  }

  static class JSONRequestMacroListEvent {

    boolean idk;
  }
}
