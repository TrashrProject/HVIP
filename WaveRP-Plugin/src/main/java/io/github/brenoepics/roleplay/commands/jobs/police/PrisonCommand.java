package io.github.brenoepics.roleplay.commands.jobs.police;

import com.eu.habbo.Emulator;
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
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import java.time.Duration;
import java.util.Arrays;

public class PrisonCommand extends Command {
  public PrisonCommand(String permission, String[] keys) { super(permission, keys); }
  @Override public boolean handle(GameClient client, String[] params) {
    Habbo officer = client.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(officer);
    if (!PoliceCommandSupport.authorize(officer, data, JobPermissions.POLICE_ARREST)) return true;
    if (params.length < 4) { officer.whisper("Usage : :prison <pseudo> <minutes> <raison>", RoomChatMessageBubbles.ALERT); return true; }
    int minutes;
    try { minutes = Integer.parseInt(params[2]); } catch (NumberFormatException ex) { minutes = 0; }
    int max = 60;
    if (minutes <= 0 || minutes > max) { officer.whisper("La duree doit etre comprise entre 1 et " + max + " minutes.", RoomChatMessageBubbles.ALERT); return true; }
    Room room = officer.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(params[1]);
    if (target == null) { officer.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT); return true; }
    if (target == officer) { officer.whisper("Vous ne pouvez pas vous emprisonner vous-meme.", RoomChatMessageBubbles.ALERT); return true; }
    if (RolePlay.getPrisonService().getJailRoom().isEmpty()) { officer.whisper("La salle de prison n'est pas configuree.", RoomChatMessageBubbles.ALERT); return true; }
    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData == null) { officer.whisper("Les donnees RP de ce joueur sont indisponibles.", RoomChatMessageBubbles.ALERT); return true; }
    if (targetData.isJailed() && targetData.getJailTime() > Emulator.getIntUnixTimestamp()) {
      officer.whisper("Ce joueur est deja en prison.", RoomChatMessageBubbles.ALERT); return true;
    }
    String reason = String.join(" ", Arrays.copyOfRange(params, 3, params.length));
    int targetId = target.getHabboInfo().getId();
    RolePlay.getEscortManager().stopEscorting(targetId);
    if (!PoliceHandcuffService.unhandcuff(target)) {
      PoliceTaserService.remove(target);
    }
    targetData.makeJailed(Duration.ofMinutes(minutes));
    PoliceCommandSupport.action(officer, "Envoie " + target.getHabboInfo().getUsername() + " en prison pour " + minutes + " minutes : " + reason);
    LiveFeed.sendGlobalAlert(LiveFeed.alert("[PRISON] " + target.getHabboInfo().getUsername()
        + " a ete emprisonne pour " + minutes + " minute(s)."));
    return true;
  }
}
