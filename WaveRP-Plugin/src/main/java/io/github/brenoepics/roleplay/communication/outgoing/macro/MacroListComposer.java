package io.github.brenoepics.roleplay.communication.outgoing.macro;

import com.google.gson.JsonArray;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.features.macro.Macro;
import java.util.Collection;

public class MacroListComposer extends OutgoingWebMessage {

  public MacroListComposer(Collection<Macro> macros) {
    super("macro_list");
    JsonArray sets = new JsonArray();
    for (Macro m : macros) {
      sets.add(m.getName());
    }
    this.data.add("sets", sets);
  }
}