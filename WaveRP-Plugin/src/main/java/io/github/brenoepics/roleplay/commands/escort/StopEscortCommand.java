package io.github.brenoepics.roleplay.commands.escort;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
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
    if (params.length > 2) { officer.whisper("Usage : :arreterescorte [pseudo]", RoomChatMessageBubbles.ALERT); return true; }
    Habbo target;
    if (params.length == 2) {
      Room room = officer.getHabboInfo().getCurrentRoom();
      target = room == null ? null : room.getHabbo(params[1]);
    } else {
      java.util.List<Integer> escorted = RolePlay.getEscortManager()
          .getEscorted(officer.getHabboInfo().getId());
      if (escorted.size() != 1) {
        officer.whisper(escorted.isEmpty() ? "Vous n'escortez personne."
            : "Precisez le pseudo du joueur a liberer.", RoomChatMessageBubbles.ALERT);
        return true;
      }
      target = Emulator.getGameEnvironment().getHabboManager().getHabbo(escorted.get(0));
    }
    if (target == null) { officer.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT); return true; }
    int targetId = target.getHabboInfo().getId();
    if (RolePlay.getEscortManager().getEscortingOfficer(targetId) != officer.getHabboInfo().getId()) {
      officer.whisper("Vous n'escortez pas ce joueur.", RoomChatMessageBubbles.ALERT); return true;
    }
    RolePlay.getEscortManager().stopEscorting(targetId);
    officer.shout("* Arr\u00eate d'escorter " + target.getHabboInfo().getUsername() + " *", RoomChatMessageBubbles.YELLOW);
    return true;
  }
}
