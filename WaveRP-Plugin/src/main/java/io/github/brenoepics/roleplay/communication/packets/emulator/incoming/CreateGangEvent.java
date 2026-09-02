package io.github.brenoepics.roleplay.communication.packets.emulator.incoming;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangDataComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangNoticeComposer;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.OrganizationType;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.SQLException;

public class CreateGangEvent extends MessageHandler {
  @Override
  public void handle() {
    if (this.client == null || this.client.getHabbo() == null) return;
    String name = clean(this.packet.readString());
    String status = clean(this.packet.readString());
    int kind = this.packet.readInt();
    int primary = this.packet.readInt();
    int secondary = this.packet.readInt();
    Habbo habbo = this.client.getHabbo();

    OrganizationType type = fromClientKind(kind);
    if (type == null) {
      fail("Type d'organisation invalide.");
      return;
    }
    if (name.length() < 3 || name.length() > 24) {
      fail("Le nom doit contenir entre 3 et 24 caractères.");
      return;
    }
    if (status.length() > 64) {
      fail("Le statut ne peut pas dépasser 64 caractères.");
      return;
    }
    int userId = habbo.getHabboInfo().getId();
    if (RolePlay.getOrganizationManager().getUserOrganization(userId) != null) {
      fail("Vous appartenez déjà à une organisation.");
      return;
    }
    if (RolePlay.getOrganizationManager().getOrganization(name) != null) {
      fail("Une organisation porte déjà ce nom.");
      return;
    }

    String configKey = type.name().toLowerCase();
    int cost = Emulator.getConfig().getInt("features.organizations." + configKey + ".price", 20);
    int requiredKills = Emulator.getConfig().getInt("features.organizations." + configKey + ".kills", 0);
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    int kills = avatar == null ? 0 : avatar.getCombatStats().getKills();
    if (kills < requiredKills) {
      fail("Il vous faut " + requiredKills + " éliminations pour créer cette organisation.");
      return;
    }
    if (habbo.getHabboInfo().getCurrencyAmount(200) < cost) {
      fail("Vous n'avez pas assez d'argent sur votre compte.");
      return;
    }

    if (!RolePlay.getOrganizationManager().createOrganization(userId, name, type)) {
      fail("La création a échoué. Réessayez dans quelques instants.");
      return;
    }
    Organization organization = RolePlay.getOrganizationManager().getOrganization(name);
    if (organization == null || !saveAppearance(organization.getId(), status, primary, secondary)) {
      if (organization != null) RolePlay.getOrganizationManager().disbandOrganization(organization.getId());
      fail("La création a échoué pendant l'enregistrement.");
      return;
    }

    habbo.getHabboInfo().addCurrencyAmount(200, -cost);
    if (avatar != null) avatar.setOrganizationId(organization.getId());
    this.client.sendResponse(new GangNoticeComposer(1, "Organisation créée.",
        "Bienvenue dans " + name, "Votre organisation est maintenant active.", primary, secondary));
    this.client.sendResponse(new GangDataComposer(habbo));
  }

  private boolean saveAppearance(int id, String status, int primary, int secondary) {
    try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
        PreparedStatement statement = connection.prepareStatement(
            "UPDATE rp_organizations SET status = ?, primary_color = ?, secondary_color = ? WHERE id = ?")) {
      statement.setString(1, status);
      statement.setInt(2, primary);
      statement.setInt(3, secondary);
      statement.setInt(4, id);
      return statement.executeUpdate() == 1;
    } catch (SQLException exception) {
      return false;
    }
  }

  private void fail(String message) {
    this.client.sendResponse(GangNoticeComposer.error(message));
  }

  private static String clean(String value) {
    return value == null ? "" : value.trim().replaceAll("[\\r\\n\\t]+", " ");
  }

  private static OrganizationType fromClientKind(int kind) {
    if (kind == 2) return OrganizationType.GANG;
    if (kind == 3) return OrganizationType.CARTEL;
    if (kind == 4) return OrganizationType.MAFIA;
    return null;
  }
}
