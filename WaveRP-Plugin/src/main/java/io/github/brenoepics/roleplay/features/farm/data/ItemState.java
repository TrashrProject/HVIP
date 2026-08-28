package io.github.brenoepics.roleplay.features.farm.data;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class ItemState {

    private static final Logger log = LoggerFactory.getLogger(ItemState.class);
    private final int id;
    private final int enable;
    private final int coolDown;
    private final int requiredItem;
    private final int requiredAmount;

    public ItemState(String states) {
        String[] split = states.split(",");
        int length = split.length;

        this.id = (length >= 1) ? Integer.parseInt(split[0]) : 0;
        this.enable = (length >= 2) ? Integer.parseInt(split[1]) : 0;
        this.coolDown = (length >= 3) ? Integer.parseInt(split[2]) : 0;
        this.requiredItem = (length >= 4) ? Integer.parseInt(split[3]) : 0;
        this.requiredAmount = (length == 5) ? Integer.parseInt(split[4]) : 0;

        if (length < 5)
        {
            log.warn("[FARM-PLUGIN] ItemState {} is missing values!", this.id);
        }
    }

    public int getId() {
        return this.id;
    }

    public int getEnable() {
        return this.enable;
    }

    public int getCooldown() {
        return this.coolDown;
    }

    public int getRequiredItem() {
        return this.requiredItem;
    }

    public int getRequiredAmount() {
        return this.requiredAmount;
    }
}
