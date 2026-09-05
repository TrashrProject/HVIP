package io.github.brenoepics.roleplay.commands.jobs.police;

import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

final class PoliceCommandSupport {
  private PoliceCommandSupport() {}

  static boolean authorize(Habbo officer, RpAvatar data, String permission) {
    if (data == null || data.getJobEntity() == null
        || !"police".equalsIgnoreCase(data.getJobEntity().getName())) {
      officer.whisper("Vous n'etes pas policier.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    if (data.getJobRankEntity() == null || !data.getJobRankEntity().hasPermission(permission)) {
      officer.whisper("Votre grade de Police ne permet pas cette action.",
          RoomChatMessageBubbles.ALERT);
      return false;
    }
    if (!data.isDuty()) {
      officer.whisper("Vous devez etre en service.", RoomChatMessageBubbles.ALERT);
      return false;
    }
    return true;
  }

  static boolean inRange(Habbo officer, Habbo target, int range) {
    int x = Math.abs(officer.getRoomUnit().getX() - target.getRoomUnit().getX());
    int y = Math.abs(officer.getRoomUnit().getY() - target.getRoomUnit().getY());
    if (x <= range && y <= range) return true;
    officer.whisper("Vous devez etre a " + range + " case(s) maximum de la cible.", RoomChatMessageBubbles.ALERT);
    return false;
  }

  static void action(Habbo officer, String message) {
    officer.shout("* " + message + " *", RoomChatMessageBubbles.AMBASSADOR);
  }
}
