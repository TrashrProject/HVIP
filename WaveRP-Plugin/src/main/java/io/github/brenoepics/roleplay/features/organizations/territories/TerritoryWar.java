package io.github.brenoepics.roleplay.features.organizations.territories;

import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import java.time.Instant;
import java.util.List;
import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class TerritoryWar {
    private final Territory territory;
    private final Organization defendingOrganization;
    private final Organization attackingOrganization;
    private final List<Habbo> attackers;
    private final List<Habbo> defenders;
    private final Instant startTime = Instant.now();
    private boolean started = false;
    private int progress = 0;
    private WarStatus status = WarStatus.ATTACKED;

    public TerritoryWar( Territory territory, Organization defendingOrganization, Organization attackingOrganization) {
        this.territory = territory;
        this.defendingOrganization = defendingOrganization;
        this.attackingOrganization = attackingOrganization;
        this.attackers = RolePlay.getOrganizationManager().getOrganizationOnlineUsers(attackingOrganization);
        this.defenders = RolePlay.getOrganizationManager().getOrganizationOnlineUsers(defendingOrganization);
    }

    public int addProgress(int progress) {
        this.progress += progress;
        return this.progress;
    }
    public enum WarStatus {
        ATTACKED,
        CONTESTED,
    }
}
