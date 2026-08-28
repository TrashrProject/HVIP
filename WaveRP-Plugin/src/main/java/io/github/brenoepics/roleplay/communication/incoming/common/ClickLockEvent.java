package io.github.brenoepics.roleplay.communication.incoming.common;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;

public class ClickLockEvent extends IncomingWebMessage<ClickLockEvent.JSONClickLockEvent> {

  public ClickLockEvent() {
    super(JSONClickLockEvent.class);
  }

  @Override
  public void handle(GameClient client, JSONClickLockEvent message) {
    if (message.lock) {
      client.getHabbo().getHabboStats().cache.put("clicked_user_lock", "1");
    } else {
      client.getHabbo().getHabboStats().cache.remove("clicked_user_lock");
    }
  }

  static class JSONClickLockEvent {

    boolean lock;
  }
}
