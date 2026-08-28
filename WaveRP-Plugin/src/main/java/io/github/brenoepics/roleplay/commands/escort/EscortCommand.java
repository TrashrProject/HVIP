package io.github.brenoepics.roleplay.commands.escort;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.crime.PoliceHandcuffService;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class EscortCommand extends Command {
  public EscortCommand(String permission, String[] keys) { super(permission, keys); }

  @Override public boolean handle(GameClient client, String[] params) {
    Habbo officer = client.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(officer);
    if (data == null || data.getJobRankEntity() == null
        || !data.getJobRankEntity().hasPermission(JobPermissions.POLICE_CUFF)) {
      officer.whisper("Vous n'etes pas policier.", RoomChatMessageBubbles.ALERT); return true;
    }
    if (!data.isDuty()) { officer.whisper("Vous devez etre en service.", RoomChatMessageBubbles.ALERT); return true; }
    if (params.length != 2) { officer.whisper("Usage : :escorter <pseudo>", RoomChatMessageBubbles.ALERT); return true; }
    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) { officer.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT); return true; }
    if (target == officer) { officer.whisper("Vous ne pouvez pas vous escorter vous-m\u00eame.", RoomChatMessageBubbles.ALERT); return true; }
    if (!PoliceHandcuffService.isHandcuffed(target.getHabboInfo().getId())) {
      officer.whisper("Vous devez d'abord menotter ce joueur.", RoomChatMessageBubbles.ALERT); return true;
    }
    if (RolePlay.getEscortManager().isPrisonerEscorted(target.getHabboInfo().getId())) {
      officer.whisper("Ce joueur est deja escorte.", RoomChatMessageBubbles.ALERT); return true;
    }
    int x = Math.abs(officer.getRoomUnit().getX() - target.getRoomUnit().getX());
    int y = Math.abs(officer.getRoomUnit().getY() - target.getRoomUnit().getY());
    if (x > 1 || y > 1) { officer.whisper("Vous devez etre a cote de la cible.", RoomChatMessageBubbles.ALERT); return true; }
    if (!RolePlay.getEscortManager().startEscorting(officer, target)) {
      officer.whisper("Impossible de commencer l'escorte dans cette salle.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }
    officer.shout("* Commence \u00e0 escorter " + target.getHabboInfo().getUsername() + " *", RoomChatMessageBubbles.YELLOW);
    return true;
  }
}
