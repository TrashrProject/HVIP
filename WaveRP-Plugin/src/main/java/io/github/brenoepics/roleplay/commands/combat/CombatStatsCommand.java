package io.github.brenoepics.roleplay.commands.combat;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.CombatStats;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.text.DecimalFormat;

public class CombatStatsCommand extends Command {

  private static final DecimalFormat DECIMAL_FORMAT = new DecimalFormat("#0.00");
  private static final String TD = "</td>";
  public static final String TR = "</tr>";

  public CombatStatsCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) throws Exception {
    Habbo requester = gameClient.getHabbo();
    Room room = requester.getHabboInfo().getCurrentRoom();

    if (room == null) {
      return false;
    }

    if (strings.length < 2) {
      displayStats(requester, requester);
      return true;
    }

    String targetName = strings[1];
    Habbo target = room.getHabbo(targetName);

    if (target == null) {
      requester.whisper("Ce joueur est introuvable dans cette salle.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    displayStats(requester, target);
    return true;
  }

  private void displayStats(Habbo requester, Habbo target) {
    RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(target);

    if (rpAvatar == null) {
      requester.whisper(
          "Les donn\u00e9es RP de " + target.getHabboInfo().getUsername() + " sont introuvables.",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    CombatStats stats = rpAvatar.getCombatStats();
    StringBuilder sb = new StringBuilder();

    sb.append("<h4>Statistiques de combat de ").append(target.getHabboInfo().getUsername())
        .append("</h2>");

    sb.append("<table border='1' style='width:100%'>");

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Performances</th></tr>");
    sb.append("<tr><td>Victoires</td><td>").append(stats.getKills()).append(TD).append(TR);
    sb.append("<tr><td>D\u00e9faites</td><td>").append(stats.getDeaths()).append(TD).append(TR);
    sb.append("<tr><td>Ratio V/D</td><td>").append(DECIMAL_FORMAT.format(stats.getKdRatio()))
        .append(TD).append(
            TR);

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>D\u00e9g\u00e2ts</th></tr>");
    sb.append("<tr><td>D\u00e9g\u00e2ts inflig\u00e9s</td><td>").append(stats.getDamageDealt()).append(TD)
        .append(
            TR);
    sb.append("<tr><td>D\u00e9g\u00e2ts re\u00e7us</td><td>").append(stats.getDamageReceived()).append(TD)
        .append(
            TR);

    if (stats.getKills() > 0) {
      double avgDamagePerKill = (double) stats.getDamageDealt() / stats.getKills();
      sb.append("<tr><td>D\u00e9g\u00e2ts moyens par victoire</td><td>")
          .append(DECIMAL_FORMAT.format(avgDamagePerKill)).append(TD).append(
              TR);
    }

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Actions de combat</th></tr>");
    sb.append("<tr><td>Coups port\u00e9s</td><td>").append(stats.getPunchesThrown()).append(TD).append(
        TR);
    sb.append("<tr><td>Coups re\u00e7us</td><td>").append(stats.getPunchesReceived()).append(TD)
        .append(
            TR);

    if (stats.getArrests() > 0) {
      sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Forces de l'ordre</th></tr>");
      sb.append("<tr><td>Arrestations</td><td>").append(stats.getArrests()).append(TD).append(
          TR);
    }

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>\u00c9tat actuel</th></tr>");
    sb.append("<tr><td>Sant\u00e9</td><td>").append(rpAvatar.getHealth()).append("/")
        .append(rpAvatar.getMaxHealth()).append(TD).append(
            TR);
    sb.append("<tr><td>Protection</td><td>").append(rpAvatar.getShield()).append("/25").append(TD)
        .append(
            TR);
    sb.append("<tr><td>Faim</td><td>").append(rpAvatar.getHunger()).append("/")
        .append(rpAvatar.getMaxHunger()).append(TD).append(
            TR);

    sb.append("<tr><td>\u00c9nergie</td><td>").append(rpAvatar.getEnergy()).append("/")
        .append(rpAvatar.getMaxEnergy()).append(TD).append(
            TR);

    sb.append("</table>");
    requester.alert(sb.toString());
  }

}
