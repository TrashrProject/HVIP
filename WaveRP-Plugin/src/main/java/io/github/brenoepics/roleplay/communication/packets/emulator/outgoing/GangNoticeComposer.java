package io.github.brenoepics.roleplay.communication.packets.emulator.outgoing;

import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;

public class GangNoticeComposer extends MessageComposer {
  private final int code;
  private final String message;
  private final String title;
  private final String popupMessage;
  private final int primary;
  private final int secondary;

  public GangNoticeComposer(int code, String message, String title, String popupMessage,
      int primary, int secondary) {
    this.code = code;
    this.message = message == null ? "" : message;
    this.title = title == null ? "" : title;
    this.popupMessage = popupMessage == null ? "" : popupMessage;
    this.primary = primary;
    this.secondary = secondary;
  }

  public static GangNoticeComposer error(String message) {
    return new GangNoticeComposer(0, message, "", "", 0, 0);
  }

  @Override
  protected ServerMessage composeInternal() {
    this.response.init(6111);
    this.response.appendInt(code);
    this.response.appendString(message);
    this.response.appendString(title);
    this.response.appendString(popupMessage);
    this.response.appendInt(primary);
    this.response.appendInt(secondary);
    return this.response;
  }
}
