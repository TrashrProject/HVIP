package io.github.brenoepics.roleplay.communication;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.incoming.loaded.LoadedEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.CreateMacroEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.DeleteMacroEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.MacroSaveEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.RequestMacroEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.RequestMacroListEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.RequestMacroSetEvent;
import io.github.brenoepics.roleplay.communication.incoming.macro.SelectMacroEvent;
import io.github.brenoepics.roleplay.communication.incoming.roleplay.AcceptOfferEvent;
import io.github.brenoepics.roleplay.communication.incoming.roleplay.DeclineOfferEvent;
import io.github.brenoepics.roleplay.communication.incoming.roleplay.OpenInventoryEvent;
import io.github.brenoepics.roleplay.utilities.JsonFactory;
import lombok.extern.slf4j.Slf4j;

@Slf4j
public class CommunicationManager {

  private static CommunicationManager instance;


  private final THashMap<String, Class<? extends IncomingWebMessage>> _incomingMessages;

  static {
    try {
      instance = new CommunicationManager();
    } catch (Exception e) {
      e.printStackTrace();
    }
  }

  public CommunicationManager() {
    this._incomingMessages = new THashMap<>();
    initializeMessages();
  }

  public void initializeMessages() {
    registerMessage("js_loaded", LoadedEvent.class);
    registerMessage("open_inventory", OpenInventoryEvent.class);
    registerMessage("accept_offer", AcceptOfferEvent.class);
    registerMessage("decline_offer", DeclineOfferEvent.class);

    registerMessage("addmacro", CreateMacroEvent.class);
    registerMessage("request_macroset", RequestMacroSetEvent.class);
    registerMessage("save_macroset", MacroSaveEvent.class);
    registerMessage("request_macrolist", RequestMacroListEvent.class);
    registerMessage("selectmacro", SelectMacroEvent.class);
    registerMessage("deletemacro", DeleteMacroEvent.class);
    registerMessage("openmacro", RequestMacroEvent.class);
  }

  public void registerMessage(String key, Class<? extends IncomingWebMessage> message) {
    this._incomingMessages.put(key, message);
  }

  public THashMap<String, Class<? extends IncomingWebMessage>> getIncomingMessages() {
    return this._incomingMessages;
  }

  public static CommunicationManager getInstance() {
    if (instance == null) {
      try {
        instance = new CommunicationManager();
      } catch (Exception e) {
        log.error("[RP COMMUNICATION] Error:", e);
      }
    }
    return instance;
  }

  public boolean OnMessage(String jsonPayload, GameClient sender) {
    String header = "";
    try {
      IncomingWebMessage.JSONIncomingEvent heading = JsonFactory.getInstance()
          .fromJson(jsonPayload, IncomingWebMessage.JSONIncomingEvent.class);
      Class<? extends IncomingWebMessage> message = getInstance().getIncomingMessages()
          .get(heading.header);
      IncomingWebMessage webEvent = message.getDeclaredConstructor(new Class[0]).newInstance();
      webEvent.handle(sender,
          JsonFactory.getInstance().fromJson(heading.data.toString(), webEvent.type));
      header = heading.header;
    } catch (Exception e) {
      Emulator.getLogging().logUndefinedPacketLine("unknown message: " + jsonPayload);
    }
    return getInstance().getIncomingMessages().containsKey(header) && !header.equalsIgnoreCase(
        "js_loaded");
  }

  public void Dispose() {
    this._incomingMessages.clear();
    instance = null;
  }
}
