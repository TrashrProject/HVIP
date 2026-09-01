package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.outgoing.roleplay.PlayerHudComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.time.Duration;
import java.time.Instant;

public class UserStatsComposer extends MessageComposer {

  private final Habbo ownUser;
  private final Habbo otherUser;

  public UserStatsComposer(Habbo ownUser, Habbo otherUser) {
    this.ownUser = ownUser;
    this.otherUser = otherUser;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6004);

    appendUserStats(ownUser);

    boolean hasOther = (otherUser != null);
    this.response.appendBoolean(hasOther);
    if (hasOther) {
      appendUserStats(otherUser);
    }

    pushParadisePlayerHud();
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
    this.response.appendInt(avatar.getHunger());
    this.response.appendInt(avatar.getMaxHunger());

    Instant aggressionUntil = avatar.getAggressionUntil();
    int aggressionRemaining = aggressionUntil == null
        ? 0
        : (int) Math.max(0, Duration.between(Instant.now(), aggressionUntil).getSeconds());
    this.response.appendInt(aggressionRemaining);
    this.response.appendInt(Emulator.getConfig().getInt("features.aggression.seconds", 600));
  }

  /**
   * Mirror the authoritative RP stats to the browser overlay channel. This is deliberately tied to
   * the existing 6004 composer so every native stats refresh also refreshes the Paradise HUD.
   */
  private void pushParadisePlayerHud() {
    if (ownUser == null) {
      return;
    }

    GameClient client = ownUser.getClient();
    if (client == null) {
      return;
    }

    Emulator.getThreading().run(() -> {
      if (ownUser.getClient() != client) {
        return;
      }

      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(ownUser);
      if (avatar == null) {
        return;
      }

      client.sendResponse(
          new JavascriptCallbackComposer(new PlayerHudComposer(ownUser, avatar)));
    }, 10);
  }
}
