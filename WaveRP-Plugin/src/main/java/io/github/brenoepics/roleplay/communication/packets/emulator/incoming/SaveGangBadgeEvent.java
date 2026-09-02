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

public class SaveGangBadgeEvent extends MessageHandler {
  private static final int MAX_PARTS = 5;

  @Override
  public void handle() {
    if (this.client == null || this.client.getHabbo() == null) return;
    int valueCount = this.packet.readInt();
    if (valueCount < 0 || valueCount > MAX_PARTS * 3 || valueCount % 3 != 0) {
      this.client.sendResponse(GangNoticeComposer.error("Logo invalide."));
      return;
    }
    int partCount = valueCount / 3;

    StringBuilder badge = new StringBuilder();
    for (int i = 0; i < partCount; i++) {
      int key = this.packet.readInt();
      int color = this.packet.readInt();
      int position = this.packet.readInt();
      if (key < 0 || key > 99 || color < 0 || color > 99 || position < 0 || position > 9) {
        this.client.sendResponse(GangNoticeComposer.error("Une partie du logo est invalide."));
        return;
      }
      badge.append(i == 0 ? 'b' : 's');
      appendTwoDigits(badge, key);
      appendTwoDigits(badge, color);
      badge.append(position);
    }

    int userId = this.client.getHabbo().getHabboInfo().getId();
    Integer organizationId = RolePlay.getOrganizationManager().getUserOrganization(userId);
    Organization organization = organizationId == null ? null
        : RolePlay.getOrganizationManager().getOrganization(organizationId);
    if (organization == null || organization.getAdminId() != userId) {
      this.client.sendResponse(GangNoticeComposer.error(
          "Seul le chef peut modifier le logo de l'organisation."));
      return;
    }

    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(
            "UPDATE rp_organizations SET badge = ? WHERE id = ?")) {
      statement.setString(1, badge.toString());
      statement.setInt(2, organization.getId());
      if (statement.executeUpdate() != 1) throw new SQLException("Organization was not updated");
      this.client.sendResponse(GangNoticeComposer.error("Logo enregistré."));
      this.client.sendResponse(new GangDataComposer(this.client.getHabbo()));
    } catch (SQLException exception) {
      this.client.sendResponse(GangNoticeComposer.error("Impossible d'enregistrer le logo."));
    }
  }

  private static void appendTwoDigits(StringBuilder target, int value) {
    if (value < 10) target.append('0');
    target.append(value);
  }
}
