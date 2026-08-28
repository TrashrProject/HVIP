package io.github.brenoepics.roleplay.communication.outgoing.macro;


import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;

public class OpenMacroComposer extends OutgoingWebMessage {

  public OpenMacroComposer() {
    super("macro_open");
  }
}
