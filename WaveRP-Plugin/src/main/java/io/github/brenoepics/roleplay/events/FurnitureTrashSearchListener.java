package io.github.brenoepics.roleplay.events;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.plugin.EventHandler;
import com.eu.habbo.plugin.EventListener;
import com.eu.habbo.plugin.events.furniture.FurnitureToggleEvent;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ThreadLocalRandom;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * ParadiseRP trash searching.
 *
 * The WavePlus core already fires FurnitureToggleEvent before the furniture's native onClick().
 * Cancelling that event lets WaveRP turn trash furniture into an RP interaction without replacing
 * the normal behaviour of other furniture.
 */
public final class FurnitureTrashSearchListener implements EventListener {

  private static final Logger LOGGER = LoggerFactory.getLogger(FurnitureTrashSearchListener.class);

  private static final long DEFAULT_SEARCH_DELAY_MS = 4_000L;
  private static final long DEFAULT_COOLDOWN_MS = 5 * 60_000L;
  private static final int DEFAULT_MAX_DISTANCE = 2;

  // These IDs are existing non-weapon RP items from WaveRP-Plugin/sql.sql.
  // The list remains configurable through config.ini without requiring code changes.
  private static final List<Integer> DEFAULT_COMMON_LOOT = Arrays.asList(3, 9, 10); // Snack, Apple, Banana
  private static final List<Integer> DEFAULT_UNCOMMON_LOOT = Arrays.asList(11, 12, 13); // Sandwich, Burger, Pizza Slice
  private static final List<Integer> DEFAULT_RARE_LOOT = Arrays.asList(14, 15); // Taco, Sushi Roll

  private static final ConcurrentHashMap<Integer, Long> COOLDOWNS = new ConcurrentHashMap<>();
  private static final Set<Integer> ACTIVE_FURNITURE = ConcurrentHashMap.newKeySet();
  private static final Set<Integer> ACTIVE_USERS = ConcurrentHashMap.newKeySet();

  @EventHandler
  public static void onFurnitureToggle(FurnitureToggleEvent event) {
    Habbo habbo = event.habbo;
    HabboItem furniture = event.furniture;

    if (habbo == null || furniture == null || furniture.getBaseItem() == null) {
      return;
    }

    String baseName = furniture.getBaseItem().getName();
    if (!isTrashFurniture(baseName)) {
      return;
    }

    // Important: stop the native furniture interaction. This is what makes a trash item act as a
    // searchable bin instead of just a normal toggleable furni.
    event.setCancelled(true);

    Room room = habbo.getHabboInfo().getCurrentRoom();
    if (room == null || room.getHabboItem(furniture.getId()) == null || habbo.getRoomUnit() == null) {
      return;
    }

    LOGGER.info("[ParadiseRP Trash] interaction itemId={}, baseItemId={}, name={}, user={}",
        furniture.getId(), furniture.getBaseItem().getId(), baseName,
        habbo.getHabboInfo().getUsername());

    int maxDistance = Math.max(1,
        Emulator.getConfig().getInt("paradise.trash.max_distance", DEFAULT_MAX_DISTANCE));
    if (!isNear(habbo, furniture, maxDistance)) {
      habbo.whisper("Approchez-vous de la poubelle pour la fouiller.", RoomChatMessageBubbles.ALERT);
      return;
    }

    long now = System.currentTimeMillis();
    Long cooldownUntil = COOLDOWNS.get(furniture.getId());
    if (cooldownUntil != null && cooldownUntil > now) {
      habbo.whisper("Cette poubelle a deja ete fouillee recemment.", RoomChatMessageBubbles.ALERT);
      return;
    }
    if (cooldownUntil != null) {
      COOLDOWNS.remove(furniture.getId(), cooldownUntil);
    }

    int userId = habbo.getHabboInfo().getId();
    if (!ACTIVE_USERS.add(userId)) {
      habbo.whisper("Vous etes deja en train de fouiller une poubelle.", RoomChatMessageBubbles.ALERT);
      return;
    }
    if (!ACTIVE_FURNITURE.add(furniture.getId())) {
      ACTIVE_USERS.remove(userId);
      habbo.whisper("Quelqu'un fouille deja cette poubelle.", RoomChatMessageBubbles.ALERT);
      return;
    }

    long cooldownMs = Math.max(1_000L,
        Emulator.getConfig().getInt("paradise.trash.cooldown_seconds", 300) * 1000L);
    COOLDOWNS.put(furniture.getId(), now + cooldownMs);

    habbo.shout("* Fouille la poubelle... *", RoomChatMessageBubbles.NORMAL);

    long delayMs = Math.max(1_000L,
        Emulator.getConfig().getInt("paradise.trash.search_seconds", 4) * 1000L);
    Emulator.getThreading().run(
        () -> finishSearch(habbo, furniture, room, userId, maxDistance), delayMs);
  }

  private static void finishSearch(Habbo habbo, HabboItem furniture, Room originalRoom, int userId,
      int maxDistance) {
    try {
      if (habbo.getHabboInfo() == null || habbo.getRoomUnit() == null
          || habbo.getHabboInfo().getCurrentRoom() != originalRoom
          || originalRoom.getHabboItem(furniture.getId()) == null) {
        return;
      }

      if (!isNear(habbo, furniture, maxDistance)) {
        habbo.whisper("Vous vous etes trop eloigne de la poubelle.", RoomChatMessageBubbles.ALERT);
        return;
      }

      int roll = ThreadLocalRandom.current().nextInt(100);
      if (roll < 50) {
        habbo.shout("* Fouille la poubelle mais ne trouve rien *", RoomChatMessageBubbles.NORMAL);
        return;
      }

      List<Integer> pool;
      if (roll < 80) {
        pool = getConfiguredIds("paradise.trash.loot.common", DEFAULT_COMMON_LOOT);
      } else if (roll < 95) {
        pool = getConfiguredIds("paradise.trash.loot.uncommon", DEFAULT_UNCOMMON_LOOT);
      } else {
        pool = getConfiguredIds("paradise.trash.loot.rare", DEFAULT_RARE_LOOT);
      }

      RPItem reward = chooseExistingSafeItem(pool);
      if (reward == null) {
        LOGGER.warn("[ParadiseRP Trash] No valid configured RP item found in loot pool {}", pool);
        habbo.shout("* Fouille la poubelle mais ne trouve rien *", RoomChatMessageBubbles.NORMAL);
        return;
      }

      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (avatar == null || avatar.getInventory() == null) {
        return;
      }

      avatar.getInventory().addItem(habbo, reward, 1);
      habbo.shout("* Fouille la poubelle et trouve " + reward.getDisplayName() + " *",
          RoomChatMessageBubbles.NORMAL);
    } catch (Exception e) {
      LOGGER.error("[ParadiseRP Trash] Failed to finish trash search", e);
    } finally {
      ACTIVE_USERS.remove(userId);
      ACTIVE_FURNITURE.remove(furniture.getId());
    }
  }

  private static boolean isNear(Habbo habbo, HabboItem furniture, int maxDistance) {
    int dx = Math.abs(habbo.getRoomUnit().getX() - furniture.getX());
    int dy = Math.abs(habbo.getRoomUnit().getY() - furniture.getY());
    return Math.max(dx, dy) <= maxDistance;
  }

  private static boolean isTrashFurniture(String baseName) {
    if (baseName == null || baseName.isBlank()) {
      return false;
    }

    String normalized = baseName.trim().toLowerCase(Locale.ROOT);
    Set<String> exactConfigured = getConfiguredNames();
    if (exactConfigured.contains(normalized)) {
      return true;
    }

    // Habbo furni classnames vary by release. Match only trash-related class-name fragments so
    // ordinary furniture is unaffected. Exact names can be forced through paradise.trash.furniture_names.
    return normalized.contains("trash")
        || normalized.contains("garbage")
        || normalized.contains("dumpster")
        || normalized.contains("waste")
        || normalized.contains("rubbish")
        || normalized.contains("poubelle")
        || normalized.contains("recycle")
        || normalized.equals("bin")
        || normalized.startsWith("bin_")
        || normalized.endsWith("_bin")
        || normalized.contains("_bin_");
  }

  private static Set<String> getConfiguredNames() {
    String raw = Emulator.getConfig().getValue("paradise.trash.furniture_names", "");
    if (raw == null || raw.isBlank()) {
      return Collections.emptySet();
    }

    Set<String> names = new HashSet<>();
    for (String value : raw.split(",")) {
      String normalized = value.trim().toLowerCase(Locale.ROOT);
      if (!normalized.isEmpty()) {
        names.add(normalized);
      }
    }
    return names;
  }

  private static List<Integer> getConfiguredIds(String key, List<Integer> defaults) {
    String raw = Emulator.getConfig().getValue(key, "");
    if (raw == null || raw.isBlank()) {
      return defaults;
    }

    List<Integer> result = new ArrayList<>();
    for (String value : raw.split(",")) {
      try {
        result.add(Integer.parseInt(value.trim()));
      } catch (NumberFormatException ignored) {
        LOGGER.warn("[ParadiseRP Trash] Ignoring invalid item id '{}' in {}", value, key);
      }
    }
    return result.isEmpty() ? defaults : result;
  }

  private static RPItem chooseExistingSafeItem(List<Integer> ids) {
    if (ids == null || ids.isEmpty()) {
      return null;
    }

    List<RPItem> candidates = new ArrayList<>();
    for (Integer id : ids) {
      RPItem item = RolePlay.getItemManager().getItemById(id);
      if (item == null) {
        continue;
      }
      String type = item.getInteractionType();
      // Never let trash configuration accidentally hand out a weapon or drug.
      if ("weapon".equalsIgnoreCase(type) || "drug".equalsIgnoreCase(type)) {
        LOGGER.warn("[ParadiseRP Trash] Refusing unsafe loot item id={} type={}", id, type);
        continue;
      }
      candidates.add(item);
    }

    if (candidates.isEmpty()) {
      return null;
    }
    return candidates.get(ThreadLocalRandom.current().nextInt(candidates.size()));
  }
}
