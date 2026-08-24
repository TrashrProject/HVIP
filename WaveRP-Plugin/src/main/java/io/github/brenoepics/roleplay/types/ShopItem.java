package io.github.brenoepics.roleplay.types;

import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;

public class ShopItem extends InventoryItem {
    public final int cost;
    public final int amount;
    public final int currencyType;

    public ShopItem(int id, String name, String image, int cost, int amount, int currencyType) {
        super(id, name, image);
        this.cost = cost;
        this.amount = amount;
        this.currencyType = currencyType;
    }

    @Override
    public JsonObject toJson() {
        JsonObject itemJson = super.toJson();
        itemJson.add("cost", new JsonPrimitive(this.cost));
        itemJson.add("amount", new JsonPrimitive(this.amount));
        itemJson.add("currency", new JsonPrimitive(this.currencyType));
        return itemJson;
    }
}
