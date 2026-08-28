package io.github.brenoepics.roleplay.features.farm.marketplace.exception;

import com.eu.habbo.Emulator;
import lombok.Getter;

public class SellItemException extends Exception {
    public SellItemException(SIException message) {
        super(Emulator.getTexts().getValue(message.getKey(), message.name()));
    }

    public SellItemException(SIException message, Throwable cause) {
        super(Emulator.getTexts().getValue(message.getKey(), message.name()), cause);
    }


    public enum SIException {
        NOT_ENOUGH_ITEMS("commands.cmd_sell_item.error.not_enough_items"),
        ;
        @Getter
        private final String key;

        SIException(String s) {
            key = s;
        }
    }

}
