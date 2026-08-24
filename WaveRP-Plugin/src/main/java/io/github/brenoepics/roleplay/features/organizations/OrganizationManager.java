package io.github.brenoepics.roleplay.features.organizations;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.territories.Territory;
import io.github.brenoepics.roleplay.features.organizations.territories.TerritoryWar;
import io.github.brenoepics.roleplay.features.organizations.territories.WarCycle;
import io.github.brenoepics.roleplay.features.organizations.territories.WarException;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import lombok.Getter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class OrganizationManager {
    private final Logger LOGGER = LoggerFactory.getLogger(OrganizationManager.class);
    @Getter
    public final Map<Integer, Organization> organizations = new HashMap<>();
    @Getter
    public final Map<Integer, List<OrganizationMember>> organizationMembers = new HashMap<>();
    @Getter
    public final Map<Integer, List<Integer>> organizationInvites = new HashMap<>();
    @Getter
    public final Map<Integer, List<Integer>> organizationApplications = new HashMap<>();
    @Getter
    public final Map<Integer, Territory> organizationTerritories = new HashMap<>();
    @Getter
    public final Map<Integer, TerritoryWar> organizationWars = new HashMap<>();

    public OrganizationManager() {
        loadOrganizations();
        loadOrganizationMembers();
        loadTerritories();
        Emulator.getThreading().run(new WarCycle());
    }

    public void loadTerritories() {
        organizationTerritories.clear();
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `rp_territories`")) {
            try (final ResultSet set = statement.executeQuery()) {
                while (set.next()) {
                    Territory territory = new Territory(set);
                    organizationTerritories.put(territory.getRoomId(), territory);
                }
            }
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        } finally {
            LOGGER.info("[NaHabbo RolePlay] Loaded {} territories", organizations.size());
        }
    }

    public void loadOrganizations() {
        organizations.clear();
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `rp_organizations`")) {
            try (final ResultSet set = statement.executeQuery()) {
                while (set.next()) {
                    Organization organization = new Organization(set.getInt("id"), set.getString("name"), OrganizationType.valueOf(set.getString("type").toUpperCase()), set.getInt("admin_id"));
                    organizations.put(organization.getId(), organization);
                }
            }
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        } finally {
            LOGGER.info("[NaHabbo RolePlay] Loaded {} job looks", organizations.size());
        }
    }

    public void loadOrganizationMembers() {
        organizationMembers.clear();
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("SELECT * FROM `rp_organization_members`")) {
            try (final ResultSet set = statement.executeQuery()) {
                while (set.next()) {
                    addMember(set.getInt("user_id"), set.getInt("organization_id"), set.getInt("rank"), false);
                }
            }
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        } finally {
            LOGGER.info("[NaHabbo RolePlay] Loaded {} job looks", organizations.size());
        }
    }

    public boolean createOrganization(int adminId, String name, OrganizationType type) {
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("INSERT INTO `rp_organizations` (`name`, `type`, `admin_id`) VALUES (?, ?, ?)", Statement.RETURN_GENERATED_KEYS)) {
            statement.setString(1, name);
            statement.setString(2, type.name().toLowerCase());
            statement.setInt(3, adminId);
            statement.executeUpdate();

            try (ResultSet generatedKeys = statement.getGeneratedKeys()) {
                if (generatedKeys.next()) {
                    int newId = generatedKeys.getInt(1);
                    Organization newOrganization = new Organization(newId, name, type, adminId);
                    organizations.put(newId, newOrganization);
                    addMember(adminId, newId, OrganizationRank.OWNER.getId());
                    return true;
                } else {
                    LOGGER.error("[NaHabbo RolePlay] Failed to create organization, no ID obtained.");
                    return false;
                }
            }
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
        return false;
    }


    public List<Habbo> getOrganizationOnlineUsers(Organization organization) {
        List<Habbo> habbos = new ArrayList<>();
        for (OrganizationMember member : organization.getMembers()) {
            Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(member.getUserId());
            if (habbo == null) continue;
            habbos.add(habbo);
        }
        return habbos;
    }
    public void startTerritoryWar(Territory territory, Organization attackers) throws WarException {
        if (attackers == null) throw new WarException("Attackers cannot be null");

        Organization defenders = organizations.get(territory.getOrganizationOwnerId());
        if (defenders == null) {
            captureTerritory(attackers.getId(), territory.getRoomId());
            getOrganizationOnlineUsers(attackers).forEach(habbo -> habbo.whisper("Your organization has claimed a territory!", RoomChatMessageBubbles.ALERT));
            return;
        }

        TerritoryWar war = new TerritoryWar(territory, defenders, attackers);
        organizationWars.put(territory.getRoomId(), war);
    }

    public void captureTerritory(int organizationId, int roomId) {
        Territory territory = RolePlay.getOrganizationManager().getOrganizationTerritories().get(roomId);
        territory.setOrganizationOwnerId(organizationId);
        territory.getClaimCountDown().addTimeOut(0, Emulator.getConfig().getInt("features.territory.claim.cooldown", 60));
        RolePlay.getOrganizationManager().getOrganizationWars().remove(roomId);
        updateTerritoryOrganization(roomId, organizationId);
    }

    public void updateTerritoryOrganization(int roomId, int organizationId) {
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("UPDATE `rp_territories` SET `organization_id` = ? WHERE `room_id` = ?")) {
            statement.setInt(1, organizationId);
            statement.setInt(2, roomId);
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
    }

    public void addMember(int userId, int orgId, int rank) {
        addMember(userId, orgId, rank, true);
    }

    public void addMember(int userId, int orgId, int rank, boolean insertSQL) {
        OrganizationMember member = new OrganizationMember(userId, OrganizationRank.getById(rank));
        if (!organizationMembers.containsKey(orgId)) {
            List<OrganizationMember> members = new ArrayList<>();
            members.add(member);
            organizationMembers.put(orgId, members);
        } else {
            organizationMembers.get(orgId).add(member);
        }
        if (!insertSQL) return;
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("INSERT INTO `rp_organization_members` (`user_id`, `organization_id`, `rank`) VALUES (?, ?, ?)")) {
            statement.setInt(1, userId);
            statement.setInt(2, orgId);
            statement.setInt(3, rank);
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
    }

    public void removeMember(int orgId, int userId) {
        organizationMembers.get(orgId).removeIf(member -> member.getUserId() == userId);
        try {
            final Connection connection = Emulator.getDatabase().getDataSource().getConnection();
            final PreparedStatement statement = connection.prepareStatement("DELETE FROM `rp_organization_members` WHERE `user_id` = ?");
            statement.setInt(1, userId);
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
    }

    public Organization getOrganization(int id) {
        return organizations.get(id);
    }

    public Organization getOrganization(String name) {
        for (Organization organization : organizations.values()) {
            if (organization.getName().equalsIgnoreCase(name)) {
                return organization;
            }
        }
        return null;
    }

    public Integer getUserOrganization(int userId) {
        for (Map.Entry<Integer, List<OrganizationMember>> entry : organizationMembers.entrySet()) {
            if (entry.getValue().stream().anyMatch(member -> member.getUserId() == userId)) {
                return entry.getKey();
            }
        }
        return null;
    }

    public void changeName(int id, String string) {
        Organization organization = organizations.get(id);
        if (organization == null) return;
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("UPDATE `rp_organizations` SET `name` = ? WHERE `id` = ?")) {
            statement.setString(1, string);
            statement.setInt(2, id);
            statement.executeUpdate();
            organization.setName(string);
            organizations.replace(id, organization);
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
    }

    public boolean inviteUser(int userId, int orgId) {
        Integer userOrg = getUserOrganization(userId);
        if (userOrg != null) return false;

        if (!organizationInvites.containsKey(orgId)) {
            List<Integer> invites = new ArrayList<>();
            invites.add(userId);
            organizationInvites.put(orgId, invites);
        } else {
            organizationInvites.get(orgId).add(userId);
        }
        return true;
    }

    public boolean isInvited(int userId, int orgId) {
        if (!organizationInvites.containsKey(orgId)) return false;
        return organizationInvites.get(orgId).contains(userId);
    }

    public void kickUser(int userId, int orgId) {
        if (!organizationMembers.containsKey(orgId)) return;
        organizationMembers.get(orgId).remove(userId);
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("DELETE FROM `rp_organization_members` WHERE `user_id` = ? AND `organization_id` = ?")) {
            statement.setInt(1, userId);
            statement.setInt(2, orgId);
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
    }

    public void disbandOrganization(int id) {
        if (!organizations.containsKey(id)) return;
        organizations.remove(id);
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("DELETE FROM `rp_organizations` WHERE `id` = ?")) {
            statement.setInt(1, id);
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
        if (!organizationMembers.containsKey(id)) return;
        for (OrganizationMember member : organizationMembers.get(id)) {
            Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(member.getUserId());
            if (habbo == null) continue;
            RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
            if (data != null) {
                data.setOrganizationId(0);
            }
        }
        organizationMembers.remove(id);
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("DELETE FROM `rp_organization_members` WHERE `organization_id` = ?")) {
            statement.setInt(1, id);
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
    }

    public boolean rankUpUser(int userId, int orgId) {
        Organization organization = organizations.get(orgId);
        if (organization == null) return false;
        List<OrganizationMember> members = organization.getMembers();
        if (members == null || members.isEmpty()) return false;
        OrganizationMember member = members.stream().filter(m -> m.getUserId() == userId).findFirst().orElse(null);
        if (member == null || member.getRank().isAdministrator()) return false;

        int newRank = member.getRank().getId() + 1;
        organization.getMembers().replaceAll(m -> m.getUserId() == userId ? new OrganizationMember(userId, OrganizationRank.getById(newRank)) : m);
        return true;
    }

    public boolean rankDownUser(int userId, int orgId) {
        Organization organization = organizations.get(orgId);
        if (organization == null) return false;
        List<OrganizationMember> members = organization.getMembers();
        if (members == null || members.isEmpty()) return false;
        OrganizationMember member = members.stream().filter(m -> m.getUserId() == userId).findFirst().orElse(null);
        if (member == null) return false;

        int newRank = member.getRank().getId() - 1;
        organization.getMembers().replaceAll(m -> m.getUserId() == userId ? new OrganizationMember(userId, OrganizationRank.getById(newRank)) : m);
        return true;
    }

    public boolean createTerritory(OrganizationType type, int roomId) {
        if (organizationTerritories.containsKey(roomId)) return false;
        Territory territory = new Territory(roomId, 0, type);
        organizationTerritories.put(roomId, territory);
        try (final Connection connection = Emulator.getDatabase().getDataSource().getConnection(); final PreparedStatement statement = connection.prepareStatement("INSERT INTO `rp_territories` (`room_id`, `type`) VALUES (?, ?)")) {
            statement.setInt(1, roomId);
            statement.setString(2, type.name().toLowerCase());
            statement.executeUpdate();
        } catch (SQLException e) {
            LOGGER.error("[NaHabbo RolePlay]", e);
        }
        return true;
    }

    public static OrganizationType getOrganizationType(String type) {
        OrganizationType organizationType = null;
        try {
            organizationType = OrganizationType.valueOf(type.toUpperCase());
        } catch (Exception ignored) {
            //ignored because we will return null
        }
        return organizationType;
    }
}
