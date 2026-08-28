package io.github.brenoepics.roleplay.communication.packets.js;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.utilities.JsonFactory;

public class JavascriptCallbackComposer extends MessageComposer {
    private final OutgoingWebMessage webMessage;

    public JavascriptCallbackComposer(OutgoingWebMessage message) {
        this.webMessage = message;
    }

    protected ServerMessage composeInternal() {
        this.response.init(2023);
        String jsonMessage = JsonFactory.getInstance().toJson(this.webMessage).replace("/", "&#47;");
        this.response.appendString("habblet/open/" + jsonMessage);
        return this.response;
    }
}
