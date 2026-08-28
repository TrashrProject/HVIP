package io.github.brenoepics.roleplay.communication.outgoing.common;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.guilds.Guild;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.messages.outgoing.guilds.GuildFavoriteRoomUserUpdateComposer;
import com.eu.habbo.plugin.events.guilds.GuildRemovedFavoriteEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class GuildRemoveFavoriteEvent extends MessageHandler {

  @Override
  public void handle() throws Exception {
    int guildId = this.packet.readInt();

    Habbo habbo = this.client.getHabbo();
    if (habbo == null || !habbo.getHabboStats().hasGuild(guildId)) {
      return;
    }

    Guild guild = Emulator.getGameEnvironment().getGuildManager().getGuild(guildId);
    GuildRemovedFavoriteEvent favoriteEvent = new GuildRemovedFavoriteEvent(guild, habbo);
    Emulator.getPluginManager().fireEvent(favoriteEvent);
    if (favoriteEvent.isCancelled()) {
      return;
    }

    habbo.getHabboStats().guild = 0;

    if (habbo.getHabboInfo().getCurrentRoom() != null && guild != null) {
      habbo.getHabboInfo().getCurrentRoom().sendComposer(new GuildFavoriteRoomUserUpdateComposer(
          habbo.getRoomUnit(), null).compose());
    }

    RpAvatar rpAvatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
    this.client.sendResponse(new UserProfileCustomComposer(habbo, this.client, rpAvatar));
  }
}