package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class HealCommand extends Command {

  public HealCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo healer = gameClient.getHabbo();
    if (MedicalCommandSupport.requireOnDutyMedic(healer, JobPermissions.MEDICAL_HEAL) == null) {
      return true;
    }

    if (params.length != 2) {
      healer.whisper("Usage : :soigner <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Habbo target = MedicalCommandSupport.findNearbyTarget(healer, params[1], false);
    if (target == null) {
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData.isDead() || targetData.getHealth() <= 0) {
      healer.whisper("Ce joueur est inconscient. Utilisez :reanimer.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (targetData.getHealth() >= targetData.getMaxHealth()) {
      healer.whisper("Ce joueur est d\u00e9j\u00e0 en parfaite sant\u00e9.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    targetData.heal();
    targetData.resetHungry();
    targetData.updateDatabase();
    healer.shout("* Soigne " + target.getHabboInfo().getUsername() + " *",
        RoomChatMessageBubbles.NORMAL);
    return true;
  }

}
