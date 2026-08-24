package io.github.brenoepics.roleplay.commands.escort;

import static io.github.brenoepics.roleplay.features.escort.EscortManager.ESCORT_VARIABLE;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceTaserService;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class StopEscortCommand extends Command {
  public StopEscortCommand(String permission, String[] keys) { super(permission, keys); }
  @Override public boolean handle(GameClient client, String[] params) {
    Habbo officer = client.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(officer);
    if (data == null || data.getJobRankEntity() == null || !data.getJobRankEntity().hasPermission(JobPermissions.POLICE_CUFF) || !data.isDuty()) {
      officer.whisper("Vous devez etre policier et en service.", RoomChatMessageBubbles.ALERT); return true;
    }
    if (params.length != 2) { officer.whisper("Usage : :stopescort <pseudo>", RoomChatMessageBubbles.ALERT); return true; }
    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) { officer.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT); return true; }
    int targetId = target.getHabboInfo().getId();
    if (RolePlay.getEscortManager().getEscortingOfficer(targetId) != officer.getHabboInfo().getId()) {
      officer.whisper("Vous n'escortez pas ce joueur.", RoomChatMessageBubbles.ALERT); return true;
    }
    RolePlay.getEscortManager().stopEscorting(targetId);
    target.getHabboStats().cache.put(ESCORT_VARIABLE, 0);
    target.getRoomUnit().setCanWalk(!PoliceTaserService.isTased(targetId));
    officer.shout("* Arrete d'escorter " + target.getHabboInfo().getUsername() + " *", RoomChatMessageBubbles.YELLOW);
    return true;
  }
}
