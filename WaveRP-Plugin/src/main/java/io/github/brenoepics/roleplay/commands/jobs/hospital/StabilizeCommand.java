package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.hospital.ems.EmsService.TreatmentResult;

public class StabilizeCommand extends Command {
  public StabilizeCommand(String permission, String[] keys) { super(permission, keys); }
  @Override public boolean handle(GameClient gameClient, String[] params) {
    Habbo medic = gameClient.getHabbo();
    Habbo patient = EmsCommandSupport.findPatient(medic, params, "Utilisation : :stabiliser <pseudo>");
    if (patient == null) return true;
    TreatmentResult result = RolePlay.getEmsService().stabilize(medic, patient);
    medic.whisper(result.message(), result.success() ? RoomChatMessageBubbles.RADIO : RoomChatMessageBubbles.ALERT);
    if (result.success()) EmsCommandSupport.announce(medic, "stabilise " + patient.getHabboInfo().getUsername());
    return true;
  }
}
import com.eu.habbo.habbohotel.commands.Command;

