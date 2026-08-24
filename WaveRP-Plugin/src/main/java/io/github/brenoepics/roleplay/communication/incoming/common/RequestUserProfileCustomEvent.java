package io.github.brenoepics.roleplay.communication.incoming.common;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.incoming.MessageHandler;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.outgoing.common.UserProfileCustomComposer;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class RequestUserProfileCustomEvent extends MessageHandler {

  @Override
  public void handle() throws Exception {
    int habboId = this.packet.readInt();
    Habbo habbo = Emulator.getGameEnvironment().getHabboManager().getHabbo(habboId);

    if (habbo != null) {
      RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
      this.client.sendResponse(new UserProfileCustomComposer(habbo, this.client, rpAvatar));
    }
  }
}

