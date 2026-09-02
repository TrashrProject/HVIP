package io.github.brenoepics.roleplay.communication.packets.emulator.incoming;

import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangDataComposer;

public class RequestGangDataEvent extends MessageHandler {
  @Override
  public void handle() {
    if (this.client == null || this.client.getHabbo() == null) return;
    // The requested id is reserved for future public profiles.
    this.packet.readInt();
    this.client.sendResponse(new GangDataComposer(this.client.getHabbo()));
  }
}
