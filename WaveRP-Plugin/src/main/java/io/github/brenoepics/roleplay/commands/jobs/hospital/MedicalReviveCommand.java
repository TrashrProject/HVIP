package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class MedicalReviveCommand extends Command {

  public MedicalReviveCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo medic = gameClient.getHabbo();
    if (MedicalCommandSupport.requireOnDutyMedic(medic, JobPermissions.MEDICAL_REVIVE) == null) {
      return true;
    }
    if (params.length != 2) {
      medic.whisper("Usage : :reanimer <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = MedicalCommandSupport.findNearbyTarget(medic, params[1], false);
    if (target == null) {
      return true;
    }
    RpAvatar targetAvatar = RolePlay.getAvatarManager().getRpAvatar(target);
    if (!targetAvatar.isDead() && targetAvatar.getHealth() > 0) {
      medic.whisper("Ce joueur est d\u00e9j\u00e0 conscient.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    targetAvatar.heal();
    RolePlay.getHospitalService().finishHealing(target);
    targetAvatar.updateDatabase();
    medic.shout("* R\u00e9anime " + target.getHabboInfo().getUsername() + " *",
        RoomChatMessageBubbles.NORMAL);
    target.whisper("Vous avez \u00e9t\u00e9 r\u00e9anim\u00e9 par "
        + medic.getHabboInfo().getUsername() + ".", RoomChatMessageBubbles.NORMAL);
    return true;
  }
}
