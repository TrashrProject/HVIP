package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobsDelegate;

final class EmsCommandSupport {

  private EmsCommandSupport() {
  }

  static boolean requireMedic(Habbo habbo, String permission) {
    if (RolePlay.getEmsService().isMedicOnDuty(habbo, permission)) {
      return true;
    }
    habbo.whisper("Vous n'avez pas l'autorisation EMS requise ou n'etes pas en service.",
        RoomChatMessageBubbles.ALERT);
    return false;
  }

  static Habbo findPatient(Habbo medic, String[] params, String usage) {
    if (params.length < 2 || medic.getHabboInfo().getCurrentRoom() == null) {
      medic.whisper(usage, RoomChatMessageBubbles.ALERT);
      return null;
    }
    Habbo patient = medic.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (patient == null || patient == medic) {
      medic.whisper("Ce patient est introuvable dans l'appartement.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    return patient;
  }

  static void announce(Habbo medic, String message) {
    medic.getHabboInfo().getCurrentRoom().sendComposer(
        JobsDelegate.getRoomUserShoutComposer("* " + message + " *", medic).compose());
  }
}

