package io.github.brenoepics.roleplay.features.organizations;

import lombok.Getter;
import lombok.Setter;

@Getter
@Setter
public class OrganizationMember {
    private final int userId;
    private final OrganizationRank rank;

    public OrganizationMember(int userId, OrganizationRank rank) {
        this.userId = userId;
        this.rank = rank;
    }
}
