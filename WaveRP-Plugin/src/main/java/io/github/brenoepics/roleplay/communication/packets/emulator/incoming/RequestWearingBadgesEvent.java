package io.github.brenoepics.roleplay.communication.packets.emulator.incoming;


import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.inventory.BadgesComponent;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.messages.outgoing.users.UserBadgesComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.TargetLockComposer;
import io.github.brenoepics.roleplay.features.targetlock.TargetLockService;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class RequestWearingBadgesEvent extends MessageHandler {

  @Override
  public void handle() throws Exception {
    int userId = this.packet.readInt();
    Habbo habbo = Emulator.getGameServer().getGameClientManager().getHabbo(userId);

    if (habbo == null || habbo.getHabboInfo() == null || habbo.getInventory() == null
        || habbo.getInventory().getBadgesComponent() == null) {
      this.client.sendResponse(
          new UserBadgesComposer(BadgesComponent.getBadgesOfflineHabbo(userId), userId));
      return;
    }

    this.client.sendResponse(
        new UserBadgesComposer(habbo.getInventory().getBadgesComponent().getWearingBadges(),
            habbo.getHabboInfo().getId()));

    String targetName = habbo.getHabboInfo().getUsername();
    Habbo requester = this.client.getHabbo();
    if (!TargetLockService.isTargetLocked(requester, habbo)) {
      TargetLockService.setClickedUser(requester, targetName);

      ServerMessage composer = new TargetLockComposer(targetName, false).compose();
      this.client.sendResponse(composer);
    }

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(this.client.getHabbo());
    data.updateLife();
  }

}