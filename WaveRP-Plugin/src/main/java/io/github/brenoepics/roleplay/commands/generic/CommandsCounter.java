package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.Emulator;
import io.github.brenoepics.roleplay.utilities.types.CountDown;
import java.util.HashMap;

public class CommandsCounter {
    private final HashMap<String, CountDown> timeouts = new HashMap<>();

    public static final Integer OFFER_TIMEOUT = Emulator.getConfig().getInt("features.offer_timeout", 5);
    public static final Integer TAZOR_TIMEOUT = Emulator.getConfig().getInt("features.tazor_timeout", 5);
    public static final Integer ARREST_TIMEOUT = Emulator.getConfig().getInt("features.arrest_timeout", 5);
    public static final Integer SHOOT_TIMEOUT = Emulator.getConfig().getInt("features.shoot_timeout", 3);
    public static final Integer ROB_TIMEOUT = Emulator.getConfig().getInt("features.rob_timeout", 3);
    public static final Integer HIT_TIMEOUT = Emulator.getConfig().getInt("features.hit_timeout", 3);
    public static final Integer APPLY_TIMEOUT = Emulator.getConfig().getInt("features.apply_timeout", 3);
    public static final Integer HELP_TIMEOUT = Emulator.getConfig().getInt("features.help_timeout", 5);
    public static final Integer PASSIVE_TIMEOUT = Emulator.getConfig().getInt("features.passive_timeout", 60);
    public static final Integer DEFAULT_SEND_HOME_TIME = Emulator.getConfig().getInt("features.default.send_home.minutes", 5);
    public static final Integer SEND_HOME_MIN = Emulator.getConfig().getInt("features.send_home.min", 1);
    public static final Integer SEND_HOME_MAX = Emulator.getConfig().getInt("features.send_home.max", 5);

    public CountDown getCoolDown(String command) {
        timeouts.putIfAbsent(command, new CountDown());
        return timeouts.get(command);
    }

}
