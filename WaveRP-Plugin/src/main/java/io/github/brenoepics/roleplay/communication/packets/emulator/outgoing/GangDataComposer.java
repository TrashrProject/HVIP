package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.OrganizationMember;
import io.github.brenoepics.roleplay.features.organizations.OrganizationRank;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.Collections;
import java.util.List;

/** Native packet consumed by the Paradise organization window. */
public class GangDataComposer extends MessageComposer {
  private final Habbo habbo;

  public GangDataComposer(Habbo habbo) {
    this.habbo = habbo;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6110);
    int userId = habbo.getHabboInfo().getId();
    Integer organizationId = RolePlay.getOrganizationManager().getUserOrganization(userId);
    Organization organization = organizationId == null ? null
        : RolePlay.getOrganizationManager().getOrganization(organizationId);
    this.response.appendBoolean(organization != null);
    if (organization == null) {
      appendCreationData();
      return this.response;
    }

    OrganizationDetails details = loadDetails(organization);
    OrganizationMember ownMember = findMember(organization, userId);
    boolean owner = organization.getAdminId() == userId;
    boolean administrator = owner || (ownMember != null && ownMember.getRank().isAdministrator());

    this.response.appendInt(organization.getId());
    this.response.appendString(organization.getName());
    this.response.appendString(details.status);
    this.response.appendInt(toClientKind(organization.getType()));
    this.response.appendInt(details.primary);
    this.response.appendInt(details.secondary);
    this.response.appendInt(1); // level
    this.response.appendInt(0); // experience
    this.response.appendInt(100); // experience required
    this.response.appendInt(0); // kills
    this.response.appendInt(0); // cop kills
    this.response.appendInt(0); // heists
    this.response.appendInt(0); // jailbreaks
    this.response.appendInt(0); // turfs
    this.response.appendInt(0); // earned
    this.response.appendBoolean(true);
    this.response.appendBoolean(owner);
    this.response.appendBoolean(owner);
    this.response.appendBoolean(owner);
    this.response.appendBoolean(administrator);
    this.response.appendBoolean(administrator);
    this.response.appendBoolean(administrator);
    this.response.appendBoolean(administrator);

    appendPermissionKeys();
    appendRoles();
    appendMembers(organization);
    this.response.appendString(details.badge);
    this.response.appendBoolean(false); // no upgrade configured yet
    return this.response;
  }

  private void appendCreationData() {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    this.response.appendInt(avatar == null ? 0 : avatar.getCombatStats().getKills());
    this.response.appendInt(3);
    appendRequirement(2, OrganizationType.GANG);
    appendRequirement(4, OrganizationType.MAFIA);
    appendRequirement(3, OrganizationType.CARTEL);
    this.response.appendInt(0); // invitations
  }

  private void appendRequirement(int kind, OrganizationType type) {
    String key = type.name().toLowerCase();
    this.response.appendInt(kind);
    this.response.appendInt(Emulator.getConfig().getInt("features.organizations." + key + ".price", 20));
    this.response.appendInt(Emulator.getConfig().getInt("features.organizations." + key + ".kills", 0));
  }

  private void appendPermissionKeys() {
    String[] permissions = {"Gérer les rôles", "Modifier l'organisation", "Inviter",
        "Promouvoir", "Rétrograder", "Exclure"};
    this.response.appendInt(permissions.length);
    for (int i = 0; i < permissions.length; i++) {
      this.response.appendInt(i + 1);
      this.response.appendString(permissions[i]);
    }
  }

  private void appendRoles() {
    this.response.appendInt(3);
    appendRole(0, "Chef", new int[] {1, 2, 3, 4, 5, 6});
    appendRole(2, "Bras droit", new int[] {3, 4, 5, 6});
    appendRole(1, "Membre", new int[0]);
  }

  private void appendRole(int level, String name, int[] permissions) {
    this.response.appendInt(level);
    this.response.appendString(name);
    this.response.appendInt(permissions.length);
    for (int permission : permissions) this.response.appendInt(permission);
  }

  private void appendMembers(Organization organization) {
    List<OrganizationMember> members = organization.getMembers();
    if (members == null) members = Collections.emptyList();
    this.response.appendInt(members.size());
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(
            "SELECT username, look FROM users WHERE id = ?")) {
      for (OrganizationMember member : members) {
        String username = "Joueur " + member.getUserId();
        String look = "";
        statement.setInt(1, member.getUserId());
        try (ResultSet set = statement.executeQuery()) {
          if (set.next()) {
            username = set.getString("username");
            look = set.getString("look");
          }
        }
        this.response.appendInt(member.getUserId());
        this.response.appendString(username);
        this.response.appendString(look);
        this.response.appendInt(member.getRank() == OrganizationRank.OWNER ? 0 : member.getRank().getId());
      }
    } catch (SQLException exception) {
      throw new IllegalStateException("Unable to compose organization members", exception);
    }
  }

  private OrganizationMember findMember(Organization organization, int userId) {
    List<OrganizationMember> members = organization.getMembers();
    if (members == null) return null;
    for (OrganizationMember member : members) {
      if (member.getUserId() == userId) return member;
    }
    return null;
  }

  private OrganizationDetails loadDetails(Organization organization) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(
            "SELECT status, primary_color, secondary_color, badge FROM rp_organizations WHERE id = ?")) {
      statement.setInt(1, organization.getId());
      try (ResultSet set = statement.executeQuery()) {
        if (set.next()) return new OrganizationDetails(set.getString("status"),
            set.getInt("primary_color"), set.getInt("secondary_color"), set.getString("badge"));
      }
    } catch (SQLException exception) {
      throw new IllegalStateException("Unable to load organization details", exception);
    }
    return new OrganizationDetails("", 0, 0, "");
  }

  public static int toClientKind(OrganizationType type) {
    if (type == OrganizationType.GANG) return 2;
    if (type == OrganizationType.CARTEL) return 3;
    return 4;
  }

  private static class OrganizationDetails {
    private final String status;
    private final int primary;
    private final int secondary;
    private final String badge;

    private OrganizationDetails(String status, int primary, int secondary, String badge) {
      this.status = status == null ? "" : status;
      this.primary = primary;
      this.secondary = secondary;
      this.badge = badge == null ? "" : badge;
    }
  }
}
