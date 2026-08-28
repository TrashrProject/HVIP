package io.github.brenoepics.roleplay.types;

import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;

public class InventoryItem {

    public final int id;
    public final String name;
    public final String image;

    public InventoryItem(int id, String name, String image) {
        this.id = id;
        this.name = name;
        this.image = image;
    }

    public JsonObject toJson()
    {
        JsonObject itemJson = new JsonObject();
        itemJson.add("id", new JsonPrimitive(this.id));
        itemJson.add("name", new JsonPrimitive(this.name));
        itemJson.add("code", new JsonPrimitive(this.image));
        return itemJson;
    }
}
