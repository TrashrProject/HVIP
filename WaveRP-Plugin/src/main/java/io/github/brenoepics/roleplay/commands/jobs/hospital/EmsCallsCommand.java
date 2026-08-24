package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.hospital.ems.EmsCall;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import java.util.List;

public class EmsCallsCommand extends Command {

  public EmsCallsCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo habbo = gameClient.getHabbo();
    if (!RolePlay.getEmsService().isMedicOnDuty(habbo, JobPermissions.MEDICAL_AMBULANCE)
        && !RolePlay.getEmsService().isMedicOnDuty(habbo, JobPermissions.MEDICAL_DISPATCH)) {
      habbo.whisper("Vous devez etre EMS en service.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    List<EmsCall> calls = RolePlay.getEmsService().getActiveCalls();
    if (calls.isEmpty()) {
      habbo.whisper("Aucun appel EMS en attente.", RoomChatMessageBubbles.RADIO);
      return true;
    }
    habbo.whisper("Appels EMS actifs (" + calls.size() + ") :", RoomChatMessageBubbles.RADIO);
    for (EmsCall call : calls) {
      String assigned = call.assignedMedicName() == null ? "libre"
          : "pris par " + call.assignedMedicName();
      habbo.whisper("#" + call.id() + " | " + call.callerName() + " | "
          + call.roomName() + " [" + call.roomId() + "] | " + call.reason() + " | " + assigned,
          RoomChatMessageBubbles.RADIO);
    }
    return true;
  }
}
import com.eu.habbo.habbohotel.commands.Command;

