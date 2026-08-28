package io.github.brenoepics.roleplay.communication.incoming.loaded;


import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.UserStatsComposer;
import io.github.brenoepics.roleplay.communication.packets.js.JavascriptCallbackComposer;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class LoadedEvent extends IncomingWebMessage<LoadedEvent.JSONLoadedEvent> {

  public LoadedEvent() {
    super(JSONLoadedEvent.class);
  }

  public void handle(GameClient client, JSONLoadedEvent message) {
    Habbo habbo = client.getHabbo();
    if (habbo == null) {
      return;
    }
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    if (data == null) {
      return;
    }

    data.updateClientData();
  }

  static class JSONLoadedEvent {

    boolean idk;
  }
}
