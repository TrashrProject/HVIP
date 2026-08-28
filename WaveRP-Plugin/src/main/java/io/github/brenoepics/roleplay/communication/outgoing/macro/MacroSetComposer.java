package io.github.brenoepics.roleplay.communication.outgoing.macro;

import com.google.gson.JsonObject;
import com.google.gson.JsonPrimitive;
import io.github.brenoepics.roleplay.communication.outgoing.OutgoingWebMessage;
import io.github.brenoepics.roleplay.features.macro.Macro;
import java.util.Map;

public class MacroSetComposer extends OutgoingWebMessage {

  public MacroSetComposer(Macro macro) {
    super("macro_set");

    this.serializeJson(true, macro);
  }

  public MacroSetComposer(boolean isEnabled, Macro macro) {
    super("macro_set");

    this.serializeJson(isEnabled, macro);
  }

  public MacroSetComposer() {
    super("macro_set");
    this.data.add("isEnabled", new JsonPrimitive(false));
  }


  private void serializeJson(boolean isEnabled, Macro macro) {
    this.data.add("isEnabled", new JsonPrimitive(isEnabled));
    this.data.add("id", new JsonPrimitive(macro.getId()));
    this.data.add("name", new JsonPrimitive(macro.getName()));
    JsonObject jsonSets = new JsonObject();
    for (Map.Entry<String, String> c : macro.getTexts().entrySet()) {
      jsonSets.add(c.getKey(), new JsonPrimitive(c.getValue()));
    }
    this.data.add("sets", jsonSets);
  }
}
