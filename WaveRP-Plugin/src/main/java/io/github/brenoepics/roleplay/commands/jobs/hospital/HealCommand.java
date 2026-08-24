package io.github.brenoepics.roleplay.commands.jobs.hospital;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.job.JobPermissions;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class HealCommand extends Command {

  public HealCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo healer = gameClient.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(healer);

    if (!data.getJobRankEntity().hasPermission(JobPermissions.MEDICAL_HEAL)) {
      healer.whisper("You are not a Medic!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!data.isDuty()) {
      healer.whisper("You are not on Duty", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 2) {
      healer.whisper(":heal <user>", RoomChatMessageBubbles.ALERT);
      return true;
    }
    Habbo target = healer.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (target == null) {
      healer.whisper("Player " + params[1] + " not found", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (target == healer) {
      healer.whisper("You cannot heal yourself!", RoomChatMessageBubbles.ALERT);
      return true;
    }

    int distanceX = target.getRoomUnit().getX() - healer.getRoomUnit().getX();
    int distanceY = target.getRoomUnit().getY() - healer.getRoomUnit().getY();

    if (distanceX < -1 || distanceX > 1 || distanceY < -1 || distanceY > 1) {
      healer.whisper(Emulator.getTexts().getValue("commands.error.cmd_pull.cant_reach")
          .replace("%user%", params[1]), RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData.getHealth() >= 100) {
      healer.whisper("The user you tried healing is already fully healed");
      return true;
    }

    targetData.heal();
    targetData.resetHungry();
    healer.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(
        new RoomChatMessage("Begins healing  " + target.getHabboInfo().getUsername() + "*",
            healer, healer, RoomChatMessageBubbles.NORMAL)).compose());
    return true;
  }

}
