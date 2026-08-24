package io.github.brenoepics.roleplay.commands.jobs.police;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceTaserService;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class TazorCommand extends Command {
  public TazorCommand(String permission, String[] keys) { super(permission, keys); }

  @Override
  public boolean handle(GameClient client, String[] params) {
    Habbo officer = client.getHabbo();
    RpAvatar officerData = RolePlay.getAvatarManager().getRpAvatar(officer);
    if (!PoliceCommandSupport.authorize(officer, officerData, JobPermissions.POLICE_TAZE)) return true;
    if (params.length != 2) {
      officer.whisper("Usage : :taser <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) {
      officer.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (target == officer) {
      officer.whisper("Vous ne pouvez pas vous taser vous-meme.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!PoliceCommandSupport.inRange(officer, target, 2)) return true;
    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData == null || targetData.isDead()) {
      officer.whisper("Vous ne pouvez pas taser une personne morte.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (!PoliceTaserService.taser(target)) {
      officer.whisper("Ce joueur est deja sous l'effet du taser.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    PoliceCommandSupport.action(officer, "Utilise son taser sur " + target.getHabboInfo().getUsername());
    officerData.executeAction();
    return true;
  }
}
