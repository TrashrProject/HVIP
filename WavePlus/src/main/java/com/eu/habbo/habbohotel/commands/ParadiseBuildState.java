package com.eu.habbo.habbohotel.commands;

import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;

import java.util.concurrent.ConcurrentHashMap;

/**
 * Per-session building helpers used by :setz and :buildmode.
 * The state is intentionally not persisted: reconnecting restores normal placement.
 */
public final class ParadiseBuildState {
    private static final ConcurrentHashMap<Integer, State> STATES = new ConcurrentHashMap<>();

    private ParadiseBuildState() {
    }

    private static State state(Habbo habbo) {
        return STATES.computeIfAbsent(habbo.getHabboInfo().getId(), ignored -> new State());
    }

    public static void setForcedZ(Habbo habbo, Double height) {
        State state = state(habbo);
        state.forcedZ = height;
        state.buildMode = false;
        cleanup(habbo, state);
    }

    public static Double getForcedZ(Habbo habbo) {
        State state = STATES.get(habbo.getHabboInfo().getId());
        if (state == null) {
            return null;
        }
        return state.buildMode ? 0.0D : state.forcedZ;
    }

    public static boolean toggleBuildMode(Habbo habbo) {
        State state = state(habbo);
        state.buildMode = !state.buildMode;
        return state.buildMode;
    }

    public static boolean isBuildMode(Habbo habbo) {
        State state = STATES.get(habbo.getHabboInfo().getId());
        return state != null && state.buildMode;
    }

    public static void clear(Habbo habbo) {
        STATES.remove(habbo.getHabboInfo().getId());
    }

    public static void applyHeight(Room room, HabboItem item, Habbo habbo) {
        if (room == null || item == null || habbo == null) {
            return;
        }

        Double height = getForcedZ(habbo);
        if (height == null) {
            return;
        }

        item.setZ(height);
        item.needsUpdate(true);
        room.updateItem(item);
    }

    private static void cleanup(Habbo habbo, State state) {
        if (!state.buildMode && state.forcedZ == null) {
            STATES.remove(habbo.getHabboInfo().getId());
        }
    }

    private static final class State {
        private volatile Double forcedZ;
        private volatile boolean buildMode;
    }
}
