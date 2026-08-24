package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.incoming.macro.RequestMacroSetEvent.JSONRequestMacroSetEvent;
import io.github.brenoepics.roleplay.communication.outgoing.macro.MacroSetComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.macro.Macro;
import java.util.Map;

public class RequestMacroSetEvent extends IncomingWebMessage<JSONRequestMacroSetEvent> {

  public RequestMacroSetEvent() {
    super(JSONRequestMacroSetEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONRequestMacroSetEvent message) {
    if (client.getHabbo() == null) {
      return;
    }

    Map<Integer, Macro> macros = RolePlay.getMacroManager()
        .getUserMacros(client.getHabbo().getHabboInfo().getId());
    if (macros == null || macros.isEmpty()) {
      client.sendResponse(new JavascriptCallbackComposer(new MacroSetComposer()));
      return;
    }
    int defaultMacro = (int) client.getHabbo().getHabboStats().cache.get("macro");
    MacroSetComposer macrosetcomposer = new MacroSetComposer(
        macros.values().stream().filter(m -> m.getId() == defaultMacro).findAny()
            .orElse(macros.get(0)));
    client.sendResponse(new JavascriptCallbackComposer(macrosetcomposer));
  }

  static class JSONRequestMacroSetEvent {

    boolean idk;
  }
}
