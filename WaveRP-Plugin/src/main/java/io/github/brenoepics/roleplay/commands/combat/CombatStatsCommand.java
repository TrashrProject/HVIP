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
      requester.whisper("User not found in this room!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    displayStats(requester, target);
    return true;
  }

  private void displayStats(Habbo requester, Habbo target) {
    RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(target);

    if (rpAvatar == null) {
      requester.whisper(
          "Cannot find roleplay data for " + target.getHabboInfo().getUsername() + "!",
          RoomChatMessageBubbles.ALERT);
      return;
    }

    CombatStats stats = rpAvatar.getCombatStats();
    StringBuilder sb = new StringBuilder();

    sb.append("<h4>Combat Statistics for ").append(target.getHabboInfo().getUsername())
        .append("</h2>");

    sb.append("<table border='1' style='width:100%'>");

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Combat Performance</th></tr>");
    sb.append("<tr><td>Kills</td><td>").append(stats.getKills()).append(TD).append(TR);
    sb.append("<tr><td>Deaths</td><td>").append(stats.getDeaths()).append(TD).append(TR);
    sb.append("<tr><td>K/D Ratio</td><td>").append(DECIMAL_FORMAT.format(stats.getKdRatio()))
        .append(TD).append(
            TR);

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Damage Statistics</th></tr>");
    sb.append("<tr><td>Total Damage Dealt</td><td>").append(stats.getDamageDealt()).append(TD)
        .append(
            TR);
    sb.append("<tr><td>Total Damage Received</td><td>").append(stats.getDamageReceived()).append(TD)
        .append(
            TR);

    if (stats.getKills() > 0) {
      double avgDamagePerKill = (double) stats.getDamageDealt() / stats.getKills();
      sb.append("<tr><td>Avg. Damage Per Kill</td><td>")
          .append(DECIMAL_FORMAT.format(avgDamagePerKill)).append(TD).append(
              TR);
    }

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Combat Actions</th></tr>");
    sb.append("<tr><td>Punches Thrown</td><td>").append(stats.getPunchesThrown()).append(TD).append(
        TR);
    sb.append("<tr><td>Punches Received</td><td>").append(stats.getPunchesReceived()).append(TD)
        .append(
            TR);

    if (stats.getArrests() > 0) {
      sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Law Enforcement</th></tr>");
      sb.append("<tr><td>Total Arrests</td><td>").append(stats.getArrests()).append(TD).append(
          TR);
    }

    sb.append("<tr><th colspan='2' style='background-color:#E0E0E0'>Current Status</th></tr>");
    sb.append("<tr><td>Health</td><td>").append(rpAvatar.getHealth()).append("/")
        .append(rpAvatar.getMaxHealth()).append(TD).append(
            TR);
    sb.append("<tr><td>Shield</td><td>").append(rpAvatar.getShield()).append("/25").append(TD)
        .append(
            TR);
    sb.append("<tr><td>Hunger</td><td>").append(rpAvatar.getHunger()).append("/")
        .append(rpAvatar.getMaxHunger()).append(TD).append(
            TR);

    sb.append("<tr><td>Energy</td><td>").append(rpAvatar.getEnergy()).append("/")
        .append(rpAvatar.getMaxEnergy()).append(TD).append(
            TR);

    sb.append("</table>");
    requester.alert(sb.toString());
  }

}