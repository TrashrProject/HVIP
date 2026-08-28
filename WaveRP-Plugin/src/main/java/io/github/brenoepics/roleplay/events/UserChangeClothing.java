package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.users.UserSavedLookEvent;
import io.github.brenoepics.roleplay.features.items.interactions.WardrobeCabin;
import java.util.HashSet;
import java.util.List;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class UserChangeClothing implements EventListener {

  private static final Logger log = LoggerFactory.getLogger(UserChangeClothing.class);

  @EventHandler
  public static void onUserSaveLookEvent(UserSavedLookEvent event) {
    if (event.habbo == null || event.habbo.getHabboInfo().getCurrentRoom() == null) {
      event.setCancelled(true);
      return;
    }

    if (event.habbo.getHabboInfo().getRank().hasPermission("acc_change_anywhere", false)) {
      return;
    }

    Room room = event.habbo.getHabboInfo().getCurrentRoom();
    HashSet<String> enabledRooms = new HashSet<>(List.of(
        Emulator.getConfig().getValue("roleplay.rooms.change_clothing.enabled").split(";")));
    boolean isClothingRoom = enabledRooms.contains(room.getId() + "");

    if (isClothingRoom) {
      return;
    }

    HabboItem item = room.getTopItemAt(event.habbo.getRoomUnit().getX(),
        event.habbo.getRoomUnit().getY());

    if (item == null || !item.getBaseItem().getInteractionType().getType()
        .equals(WardrobeCabin.class)) {
      event.setCancelled(true);
      event.habbo.whisper(
          Emulator.getTexts().getValue("roleplay.error.change_clothing.not_allowed"));
    }
  }
}
