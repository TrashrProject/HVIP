package com.eu.habbo.habbohotel.rooms.pathfinding.impl;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.outgoing.rooms.items.RemoveFloorItemComposer;
import com.eu.habbo.messages.outgoing.rooms.items.RoomFloorItemsComposer;
import gnu.trove.TCollections;
import gnu.trove.map.TIntObjectMap;
import gnu.trove.map.hash.TIntObjectHashMap;
import gnu.trove.set.hash.THashSet;
import java.util.Set;

public class PathUtils {

  private PathUtils() {
    throw new IllegalStateException("Utility class");
  }

  private static int internalId = -1;

  /**
   * Displays a path effect as floor items in the game client for the specified room tiles.
   * These items are temporary and will be removed after a short delay.
   *
   * @param client The game client where the effect will be displayed.
   * @param roomTiles A set of room tiles where the path effect should be applied.
   */
  public static void debugPath(GameClient client, Set<RoomTile> roomTiles) {
    Item effectItem = Emulator.getGameEnvironment().getItemManager().getItem("mutearea_sign2");

    if (effectItem != null) {
      TIntObjectMap<String> ownerNames = TCollections.synchronizedMap(new TIntObjectHashMap<>(0));
      ownerNames.put(-1, "System");
      THashSet<HabboItem> items = new THashSet<>();

      for (RoomTile tile : roomTiles) {
        internalId--;
        HabboItem item = new InteractionDefault(internalId, -1, effectItem, "1", 0, 0);
        item.setX(tile.x);
        item.setY(tile.y);
        item.setZ(tile.relativeHeight());
        items.add(item);
      }

      client.sendResponse(new RoomFloorItemsComposer(ownerNames, items));
      Emulator.getThreading().run(() -> {
        for (HabboItem item : items) {
          client.sendResponse(new RemoveFloorItemComposer(item, true));
        }
      }, 3000);
    }
  }

}
