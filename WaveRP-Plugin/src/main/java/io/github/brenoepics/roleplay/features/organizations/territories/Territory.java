package io.github.brenoepics.roleplay.features.organizations.territories;

import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;
import io.github.brenoepics.roleplay.utilities.types.CountDown;
import java.sql.ResultSet;
import java.sql.SQLException;
import lombok.Getter;
import lombok.Setter;

@Getter
public class Territory {

    private final int roomId;
    @Setter
    private int organizationOwnerId;
    @Getter
    @Setter
    private OrganizationType type;
    @Getter
    private final CountDown claimCountDown = new CountDown();

    public Territory(ResultSet set) throws SQLException {
        this.roomId = set.getInt("room_id");
        this.organizationOwnerId = set.getInt("organization_id");
        this.type = OrganizationType.valueOf(set.getString("type").toUpperCase());
    }

    public Territory(int roomId, int organizationOwnerId, OrganizationType type) {
        this.roomId = roomId;
        this.organizationOwnerId = organizationOwnerId;
        this.type = type;
    }

    public boolean isClaimed() {
        return organizationOwnerId != 0;
    }

    public boolean isClaimedBy(Organization organization) {
        return isClaimedBy(organization.getId());
    }
    public boolean isClaimedBy(int organizationOwnerIdn) {
        return this.organizationOwnerId == organizationOwnerIdn;
    }

}
