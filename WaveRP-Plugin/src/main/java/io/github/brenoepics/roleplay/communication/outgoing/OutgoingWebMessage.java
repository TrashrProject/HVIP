package io.github.brenoepics.roleplay.communication.outgoing;

import com.google.gson.JsonObject;

public class OutgoingWebMessage {
    public final String header;

    public final JsonObject data;

    public OutgoingWebMessage(String name) {
        this.header = name;
        this.data = new JsonObject();
    }
}
