package io.github.brenoepics.roleplay.communication.packets.emulator.incoming;

import com.eu.habbo.Emulator;
import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangDataComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangNoticeComposer;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;

public class SaveGangColorsEvent extends MessageHandler {
  @Override
  public void handle() {
    if (this.client == null || this.client.getHabbo() == null) return;
    int primary = this.packet.readInt();
    int secondary = this.packet.readInt();
    int userId = this.client.getHabbo().getHabboInfo().getId();
    Integer organizationId = RolePlay.getOrganizationManager().getUserOrganization(userId);
    Organization organization = organizationId == null ? null
        : RolePlay.getOrganizationManager().getOrganization(organizationId);
    if (organization == null || organization.getAdminId() != userId) {
      this.client.sendResponse(GangNoticeComposer.error(
          "Seul le chef peut modifier les couleurs de l'organisation."));
      return;
    }

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(
            "UPDATE rp_organizations SET primary_color = ?, secondary_color = ? WHERE id = ?")) {
      statement.setInt(1, primary);
      statement.setInt(2, secondary);
      statement.setInt(3, organization.getId());
      if (statement.executeUpdate() != 1) throw new SQLException("Organization was not updated");
      this.client.sendResponse(GangNoticeComposer.error("Couleurs enregistrées."));
      this.client.sendResponse(new GangDataComposer(this.client.getHabbo()));
    } catch (SQLException exception) {
      this.client.sendResponse(GangNoticeComposer.error("Impossible d'enregistrer les couleurs."));
    }
  }
}
