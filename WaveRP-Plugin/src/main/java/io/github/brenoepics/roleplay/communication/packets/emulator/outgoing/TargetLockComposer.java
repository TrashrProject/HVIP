package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;

public class TargetLockComposer extends MessageComposer {

  private final String targetName;
  private final boolean isLocked;

  public TargetLockComposer(String targetName, boolean isLocked) {
    this.targetName = targetName;
    this.isLocked = isLocked;
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6000);

    this.response.appendBoolean(this.isLocked);
    this.response.appendString(this.targetName);

    return this.response;
  }
}
