package io.github.brenoepics.roleplay.communication.packets.emulator.incoming;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangDataComposer;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.GangNoticeComposer;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.OrganizationMember;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class DeleteGangEvent extends MessageHandler {
  @Override
  public void handle() {
    if (this.client == null || this.client.getHabbo() == null) return;
    int userId = this.client.getHabbo().getHabboInfo().getId();
    Integer organizationId = RolePlay.getOrganizationManager().getUserOrganization(userId);
    Organization organization = organizationId == null ? null
        : RolePlay.getOrganizationManager().getOrganization(organizationId);
    if (organization == null || organization.getAdminId() != userId) {
      this.client.sendResponse(GangNoticeComposer.error(
          "Seul le chef peut supprimer cette organisation."));
      return;
    }

    List<OrganizationMember> currentMembers = organization.getMembers() == null
        ? Collections.emptyList() : new ArrayList<>(organization.getMembers());
    String name = organization.getName();
    RolePlay.getOrganizationManager().disbandOrganization(organization.getId());

    for (OrganizationMember member : currentMembers) {
      Habbo memberHabbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(member.getUserId());
      if (memberHabbo == null || memberHabbo.getClient() == null) continue;
      memberHabbo.getClient().sendResponse(GangNoticeComposer.error(
          "L'organisation " + name + " a été supprimée."));
      memberHabbo.getClient().sendResponse(new GangDataComposer(memberHabbo));
    }
  }
}
