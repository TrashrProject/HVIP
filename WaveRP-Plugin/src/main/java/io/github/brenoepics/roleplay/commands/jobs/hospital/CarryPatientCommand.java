package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class CarryPatientCommand extends Command {

  public CarryPatientCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo medic = gameClient.getHabbo();
    if (MedicalCommandSupport.requireOnDutyMedic(medic, JobPermissions.MEDICAL_AMBULANCE) == null) {
      return true;
    }
    if (params.length != 2) {
      medic.whisper("Usage : :porter <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (RolePlay.getEscortManager().isEscorted(medic.getHabboInfo().getId())) {
      medic.whisper("Vous transportez d\u00e9j\u00e0 un patient.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = MedicalCommandSupport.findNearbyTarget(medic, params[1], false);
    if (target == null) {
      return true;
    }
    RpAvatar targetAvatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (!targetAvatar.isDead() && targetAvatar.getHealth() >= targetAvatar.getMaxHealth()) {
      medic.whisper("Ce joueur n'est ni bless\u00e9 ni inconscient.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (RolePlay.getEscortManager().isPrisonerEscorted(target.getHabboInfo().getId())) {
      medic.whisper("Ce joueur est d\u00e9j\u00e0 transport\u00e9.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!RolePlay.getEscortManager().startEscorting(medic, target)) {
      medic.whisper("Impossible de transporter ce joueur dans cette salle.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    medic.shout("* Transporte " + target.getHabboInfo().getUsername() + " *",
        RoomChatMessageBubbles.NORMAL);
    return true;
  }
}
