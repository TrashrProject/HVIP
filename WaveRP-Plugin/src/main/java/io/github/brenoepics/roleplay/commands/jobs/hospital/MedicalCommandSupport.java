package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

final class MedicalCommandSupport {

  private MedicalCommandSupport() {
  }

  static RpAvatar requireOnDutyMedic(Habbo medic, String permission) {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(medic);
    int medicalJobId = RolePlay.getJobService().getJobByName("hospital")
        .map(job -> job.getId()).orElse(-1);

    if (avatar == null || avatar.getJobEntity() == null
        || avatar.getJobEntity().getId() != medicalJobId) {
      medic.whisper("Vous n'appartenez pas aux Services m\u00e9dicaux.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    if (!avatar.isDuty()) {
      medic.whisper("Vous devez \u00eatre en service EMS.", RoomChatMessageBubbles.ALERT);
      return null;
    }
    if (avatar.getJobRankEntity() == null
        || !avatar.getJobRankEntity().hasPermission(permission)) {
      medic.whisper("Votre grade EMS ne vous autorise pas \u00e0 effectuer cette action.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    return avatar;
  }

  static Habbo findNearbyTarget(Habbo medic, String username, boolean allowSelf) {
    Room room = medic.getHabboInfo().getCurrentRoom();
    Habbo target = room == null ? null : room.getHabbo(username);
    if (target == null || target.getRoomUnit() == null || medic.getRoomUnit() == null) {
      medic.whisper("Ce joueur est introuvable dans cette salle.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    if (!allowSelf && target == medic) {
      medic.whisper("Vous ne pouvez pas effectuer cette action sur vous-m\u00eame.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }

    int distanceX = Math.abs(target.getRoomUnit().getX() - medic.getRoomUnit().getX());
    int distanceY = Math.abs(target.getRoomUnit().getY() - medic.getRoomUnit().getY());
    if (distanceX > 1 || distanceY > 1) {
      medic.whisper("Vous devez \u00eatre \u00e0 c\u00f4t\u00e9 de ce joueur.",
          RoomChatMessageBubbles.ALERT);
      return null;
    }
    return target;
  }
}
