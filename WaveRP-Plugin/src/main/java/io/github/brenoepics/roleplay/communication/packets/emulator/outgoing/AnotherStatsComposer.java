package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class AnotherStatsComposer extends MessageComposer {

  private final Habbo otherUser;

  public AnotherStatsComposer(Habbo otherUser) {
    this.otherUser = otherUser;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6005);

    appendUserStats(otherUser);
    return this.response;
  }

  private void appendUserStats(Habbo ownUser) {
    RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(ownUser);
    this.response.appendInt(ownUser.getHabboInfo().getId());
    this.response.appendString(ownUser.getHabboInfo().getUsername());
    this.response.appendString(ownUser.getHabboInfo().getLook());
    this.response.appendInt(avatar.getHealth());
    this.response.appendInt(avatar.getMaxHealth());
    this.response.appendInt(avatar.getShield());
    this.response.appendInt(avatar.getMaxShield());
    this.response.appendInt(avatar.getEnergy());
    this.response.appendInt(avatar.getMaxEnergy());
  }
}