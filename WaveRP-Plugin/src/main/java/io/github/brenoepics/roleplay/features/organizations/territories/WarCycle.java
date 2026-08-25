package io.github.brenoepics.roleplay.features.organizations.territories;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Objects;

public class WarCycle implements Runnable {
    private static final int WAR_CYCLE_TICK = Emulator.getConfig().getInt("features.territory_war.ticks.milis", 2000);

    @Override
    public void run() {
        for (Map.Entry<Integer, TerritoryWar> entry : RolePlay.getOrganizationManager().getOrganizationWars().entrySet()) {
            TerritoryWar war = entry.getValue();
            Room room = Emulator.getGameEnvironment().getRoomManager().getRoom(entry.getKey());
            if (war == null || room == null) {
                RolePlay.getOrganizationManager().getOrganizationWars().remove(entry.getKey());
                continue;
            }

            processWar(war, room);
        }
        Emulator.getThreading().run(this, WAR_CYCLE_TICK);
    }

    private void messageTeam(List<Habbo> habboList, String message) {
        for (Habbo habbo : habboList) {
            if (habbo == null || habbo.getHabboInfo().getCurrentRoom() == null) continue;
            habbo.whisper(message, RoomChatMessageBubbles.ALERT);
        }
    }

    private void processWar(TerritoryWar war, Room room) {
        if (!war.isStarted()) {
            warStarted(war, room);
            war.setStarted(true);
        }

        int progress = war.addProgress(processProgress(war, room));
        if (progress >= 100) {
            captureTerritory(war, room);
            return;
        }

        if (progress < 0) {
            territoryDefended(war, room);
            return;
        }

        handleStatus(war, room, progress);
    }

    private void handleStatus(TerritoryWar war, Room room, int progress) {
        if (Objects.requireNonNull(war.getStatus()) == TerritoryWar.WarStatus.ATTACKED) {
            messageTeam(war.getDefenders(), "Votre territoire est attaqu\u00e9 par " + war.getAttackingOrganization().getName() + " ! (" + progress + " %)");
            messageTeam(war.getAttackers(), "Vous attaquez le territoire de " + war.getDefendingOrganization().getName() + " ! (" + progress + " %)");
        } else {
            messageTeam(war.getDefenders(), "Continuez, vous reprenez votre territoire ! (" + progress + " %)");
            messageTeam(war.getAttackers(), "Quelqu'un conteste le territoire " + room.getName() + " ! (" + progress + " %)");
        }
    }
    private int processProgress(TerritoryWar war, Room room) {
        List<Habbo> habboList = new ArrayList<>(room.getHabbos());
        int attackers = 0;
        int defenders = 0;
        for (Habbo habbo : habboList) {
            RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
            if (data.isDead() || data.isPassive()) continue;

            if (war.getDefenders().contains(habbo)) {
                defenders++;
            }

            if (war.getAttackers().contains(habbo)) {
                attackers++;
            }
        }

        if(defenders > 0 && attackers > 0) {
            war.setStatus(TerritoryWar.WarStatus.CONTESTED);
            return 0;
        }

        if (defenders > 0) {
            war.setStatus(TerritoryWar.WarStatus.CONTESTED);
            return -Emulator.getConfig().getInt("features.territory_war.tick.progress", 10);
        }

        if (attackers > 0) {
            war.setStatus(TerritoryWar.WarStatus.ATTACKED);
            return Emulator.getConfig().getInt("features.territory_war.tick.progress", 10);
        }
        return 0;
    }

    private void warStarted(TerritoryWar war, Room room) {
        messageTeam(war.getAttackers(), "Votre organisation attaque le territoire " + room.getName() + " de " + war.getDefendingOrganization().getName() + ".");
        messageTeam(war.getDefenders(), "Votre territoire " + room.getName() + " est attaqu\u00e9 par " + war.getAttackingOrganization().getName() + " !");
    }

    private void captureTerritory(TerritoryWar war, Room room) {
        messageTeam(war.getDefenders(), "Votre territoire " + room.getName() + " a \u00e9t\u00e9 captur\u00e9 par " + war.getAttackingOrganization().getName() + " !");
        messageTeam(war.getAttackers(), "Votre organisation a captur\u00e9 le territoire " + room.getName() + ".");
        RolePlay.getOrganizationManager().captureTerritory(war.getAttackingOrganization().getId(), room.getId());
    }

    private void territoryDefended(TerritoryWar war, Room room) {
        messageTeam(war.getDefenders(), "Votre territoire " + room.getName() + " a \u00e9t\u00e9 d\u00e9fendu !");
        messageTeam(war.getAttackers(), "Votre organisation n'a pas r\u00e9ussi \u00e0 capturer " + room.getName() + ".");
        RolePlay.getOrganizationManager().getOrganizationTerritories().get(room.getId()).getClaimCountDown().addTimeOut(0, Emulator.getConfig().getInt("features.territory.defended.seconds_cooldown", 60));
        RolePlay.getOrganizationManager().getOrganizationWars().remove(room.getId());
    }
}
