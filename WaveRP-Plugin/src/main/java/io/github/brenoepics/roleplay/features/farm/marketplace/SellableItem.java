package io.github.brenoepics.roleplay.features.farm.marketplace;

import lombok.Getter;

import java.util.ArrayList;
import java.util.List;

@Getter
public class SellableItem {
    private final int id;
    private final String name;
    private final int baseItem;
    private final List<Integer> rooms;
    private final int credits;
    private final int currencyType;
    private final int currencyAmount;

    public SellableItem(int id, String name, int baseItem, List<String> rooms, int credits, int currencyType, int currencyAmount) {
        this.id = id;
        this.name = name;
        this.baseItem = baseItem;
        this.credits = credits;
        this.currencyType = currencyType;
        this.currencyAmount = currencyAmount;
        this.rooms = parseRoomList(rooms);
    }

    private List<Integer> parseRoomList(List<String> rooms) {
       List<Integer> roomIds = new ArrayList<>();
         for (String room : rooms) {
              roomIds.add(Integer.parseInt(room));
         }
            return roomIds;
    }
}
