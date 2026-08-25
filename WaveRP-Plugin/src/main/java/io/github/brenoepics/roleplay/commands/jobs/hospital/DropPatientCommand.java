package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import java.util.List;

public class DropPatientCommand extends Command {

  public DropPatientCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo medic = gameClient.getHabbo();
    if (MedicalCommandSupport.requireOnDutyMedic(medic, JobPermissions.MEDICAL_AMBULANCE) == null) {
      return true;
    }
    if (params.length > 2) {
      medic.whisper("Usage : :deposer [pseudo]", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target;
    if (params.length == 2) {
      Room room = medic.getHabboInfo().getCurrentRoom();
      target = room == null ? null : room.getHabbo(params[1]);
    } else {
      List<Integer> patients = RolePlay.getEscortManager()
          .getEscorted(medic.getHabboInfo().getId());
      if (patients.size() != 1) {
        medic.whisper(patients.isEmpty() ? "Vous ne transportez aucun patient."
            : "Pr\u00e9cisez le pseudo du patient \u00e0 d\u00e9poser.",
            RoomChatMessageBubbles.ALERT);
        return true;
      }
      target = Emulator.getGameEnvironment().getHabboManager().getHabbo(patients.get(0));
    }

    if (target == null || RolePlay.getEscortManager().getEscortingOfficer(
        target.getHabboInfo().getId()) != medic.getHabboInfo().getId()) {
      medic.whisper("Vous ne transportez pas ce joueur.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RolePlay.getEscortManager().stopEscorting(target.getHabboInfo().getId());
    medic.shout("* D\u00e9pose " + target.getHabboInfo().getUsername() + " *",
        RoomChatMessageBubbles.NORMAL);
    return true;
  }
}
