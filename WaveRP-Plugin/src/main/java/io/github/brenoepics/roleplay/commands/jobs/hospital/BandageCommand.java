package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.hospital.ems.EmsService.TreatmentResult;

public class BandageCommand extends Command {
  public BandageCommand(String permission, String[] keys) { super(permission, keys); }
  @Override public boolean handle(GameClient gameClient, String[] params) {
    Habbo medic = gameClient.getHabbo();
    Habbo patient = EmsCommandSupport.findPatient(medic, params, "Utilisation : :bandage <pseudo>");
    if (patient == null) return true;
    TreatmentResult result = RolePlay.getEmsService().bandage(medic, patient);
    medic.whisper(result.message(), result.success() ? RoomChatMessageBubbles.RADIO : RoomChatMessageBubbles.ALERT);
    if (result.success()) EmsCommandSupport.announce(medic, "pose un bandage a " + patient.getHabboInfo().getUsername());
    return true;
  }
}

