package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class DiagnosticCommand extends Command {

  public DiagnosticCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo medic = gameClient.getHabbo();
    if (MedicalCommandSupport.requireOnDutyMedic(medic, JobPermissions.MEDICAL_HEAL) == null) {
      return true;
    }
    if (params.length != 2) {
      medic.whisper("Usage : :diagnostic <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = MedicalCommandSupport.findNearbyTarget(medic, params[1], true);
    if (target == null) {
      return true;
    }
    RpAvatar targetAvatar = RolePlay.getAvatarManager().getRpAvatar(target);
    int health = targetAvatar.getHealth();
    String state;
    if (targetAvatar.isDead() || health <= 0) {
      state = "Inconscient";
    } else if (health <= 25) {
      state = "\u00c9tat critique";
    } else if (health <= 50) {
      state = "Blessure grave";
    } else if (health < targetAvatar.getMaxHealth()) {
      state = "Blessure l\u00e9g\u00e8re";
    } else {
      state = "\u00c9tat stable";
    }

    medic.whisper("Diagnostic de " + target.getHabboInfo().getUsername() + " : " + health
        + "/" + targetAvatar.getMaxHealth() + " PV - " + state + ".",
        RoomChatMessageBubbles.NORMAL);
    return true;
  }
}
