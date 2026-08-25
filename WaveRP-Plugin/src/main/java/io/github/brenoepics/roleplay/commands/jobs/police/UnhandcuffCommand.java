package io.github.brenoepics.roleplay.commands.jobs.police;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class UnhandcuffCommand extends Command {

  public UnhandcuffCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient client, String[] params) {
    Habbo officer = client.getHabbo();
    RpAvatar officerData = RolePlay.getAvatarManager().getRpAvatar(officer);
    if (!PoliceCommandSupport.authorize(officer, officerData, JobPermissions.POLICE_CUFF)) {
      return true;
    }
    if (params.length != 2) {
      officer.whisper("Usage : :demenotter <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) {
      officer.whisper("Ce joueur est introuvable dans cette salle.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!PoliceCommandSupport.inRange(officer, target, 1)) return true;
    if (!PoliceHandcuffService.unhandcuff(target)) {
      officer.whisper("Ce joueur n'est pas menotte.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    PoliceCommandSupport.action(officer,
        "Retire les menottes de " + target.getHabboInfo().getUsername());
    return true;
  }
}
