package io.github.brenoepics.roleplay.communication.incoming;

import com.eu.habbo.Emulator;
import com.eu.habbo.messages.ClientMessage;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.messages.incoming.rooms.RequestRoomLoadEvent;
import com.eu.habbo.messages.outgoing.rooms.ForwardToRoomComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;

public class OverrideRequestRoomLoadEvent extends MessageHandler {

  @Override
  public void handle() throws Exception {

    ClientMessage original = this.packet.clone();

    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(this.client.getHabbo());

    int roomId = this.packet.readInt();

    int jailRoom = Emulator.getConfig().getInt("nahabbo.features.jail.roomid");
    if (data.getJailTime() > Emulator.getIntUnixTimestamp() && roomId != jailRoom && jailRoom > 0) {
      this.client.sendResponse(new ForwardToRoomComposer(jailRoom));
    } else {
      RequestRoomLoadEvent originalEvent = new RequestRoomLoadEvent();
      originalEvent.packet = original;
      originalEvent.client = this.client;
      originalEvent.handle();
    }
  }
}
