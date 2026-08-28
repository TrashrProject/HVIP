package io.github.brenoepics.roleplay.commands.jobs.police;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.crime.PoliceTaserService;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class HandcuffCommand extends Command {

  public HandcuffCommand(String permission, String[] keys) {
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
      officer.whisper("Usage : :menotter <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) {
      officer.whisper("Ce joueur est introuvable dans cette salle.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (target == officer) {
      officer.whisper("Vous ne pouvez pas vous menotter vous-meme.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!PoliceCommandSupport.inRange(officer, target, 1)) return true;
    if (!PoliceTaserService.isTased(target.getHabboInfo().getId())) {
      officer.whisper("Vous devez d'abord taser ce joueur.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!PoliceHandcuffService.handcuff(target)) {
      officer.whisper("Ce joueur est deja menotte.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    PoliceCommandSupport.action(officer, "Menotte " + target.getHabboInfo().getUsername());
    return true;
  }
}
