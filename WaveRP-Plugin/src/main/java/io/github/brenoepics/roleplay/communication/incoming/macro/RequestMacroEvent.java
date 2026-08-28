package io.github.brenoepics.roleplay.communication.incoming.macro;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.incoming.macro.RequestMacroEvent.JSONRequestMacroEvent;
import io.github.brenoepics.roleplay.communication.outgoing.macro.OpenMacroComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;

public class RequestMacroEvent extends IncomingWebMessage<JSONRequestMacroEvent> {

  public RequestMacroEvent() {
    super(JSONRequestMacroEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONRequestMacroEvent message) {
    if (client.getHabbo() == null) {
      return;
    }

    OpenMacroComposer openMacroComposer = new OpenMacroComposer();
    client.sendResponse(new JavascriptCallbackComposer(openMacroComposer));
  }

  static class JSONRequestMacroEvent {

    String idk;
  }
}
