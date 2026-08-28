package io.github.brenoepics.roleplay.communication.packets.js;

import com.eu.habbo.messages.ClientMessage;
import com.eu.habbo.messages.ICallable;
import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.communication.CommunicationManager;

public class JavascriptCallbackEvent implements ICallable {
    public void call(MessageHandler messageHandler) {
        try {
            ClientMessage clientMessage = messageHandler.packet.clone();
            String payload = clientMessage.readString();
            if (CommunicationManager.getInstance().OnMessage(payload, messageHandler.client))
                messageHandler.isCancelled = true;
        } catch (Exception ignored) {}
    }
}
