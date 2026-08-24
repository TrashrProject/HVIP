package io.github.brenoepics.roleplay.features.organizations;

import lombok.Getter;

@Getter
public enum OrganizationRank {
    OWNER(3),
    ADMIN(2),
    MEMBER(1);

    private final int id;

    OrganizationRank(int id) {
        this.id = id;
    }

		public boolean isAdministrator() {
        return this == OWNER || this == ADMIN;
    }

    public static OrganizationRank getById(int id) {
        for (OrganizationRank rank : values()) {
            if (rank.getId() == id) {
                return rank;
            }
        }
        return MEMBER;
    }
}
