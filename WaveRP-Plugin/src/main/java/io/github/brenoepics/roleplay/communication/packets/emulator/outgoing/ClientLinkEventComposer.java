package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
public class ClientLinkEventComposer extends MessageComposer {
  private final String event;
  public ClientLinkEventComposer(String event){this.event=event;}
  protected ServerMessage composeInternal(){this.response.init(2023);this.response.appendString(event);return this.response;}
}
