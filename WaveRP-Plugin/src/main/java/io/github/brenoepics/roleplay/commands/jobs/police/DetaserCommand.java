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

public class DetaserCommand extends Command {
  public DetaserCommand(String permission, String[] keys) { super(permission, keys); }
  @Override public boolean handle(GameClient client, String[] params) {
    Habbo officer = client.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(officer);
    if (!PoliceCommandSupport.authorize(officer, data, JobPermissions.POLICE_TAZE)) return true;
    if (params.length != 2) { officer.whisper("Usage : :detaser <pseudo>", RoomChatMessageBubbles.ALERT); return true; }
    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) { officer.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT); return true; }
    if (!PoliceCommandSupport.inRange(officer, target, 2)) return true;
    if (!PoliceTaserService.remove(target)) { officer.whisper("Ce joueur n'est pas sous l'effet du taser.", RoomChatMessageBubbles.ALERT); return true; }
    PoliceCommandSupport.action(officer, "Retire l'effet du taser sur " + target.getHabboInfo().getUsername());
    return true;
  }
}
