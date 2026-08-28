package io.github.brenoepics.roleplay.features.organizations;

import io.github.brenoepics.roleplay.RolePlay;
import java.util.List;
import lombok.Getter;
import lombok.Setter;

@Getter
public class Organization {

    private final int id;
    @Setter
    private String name;
    private final OrganizationType type;
    private final Integer adminId;

    public Organization(int id, String name, OrganizationType type, Integer adminId) {
        this.id = id;
        this.name = name;
        this.type = type;
        this.adminId = adminId;
    }

    public List<OrganizationMember> getMembers() {
        return RolePlay.getOrganizationManager().getOrganizationMembers().get(id);
    }
}
