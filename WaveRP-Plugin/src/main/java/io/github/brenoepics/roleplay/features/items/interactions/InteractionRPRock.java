package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.time.Instant;

public class InteractionRPRock extends InteractionDefault {

  private Instant finish = Instant.now();

  public InteractionRPRock(ResultSet set, Item baseItem) throws SQLException {
    super(set, baseItem);
  }

  public InteractionRPRock(int id, int userId, Item item, String extradata, int limitedStack,
      int limitedSells) {
    super(id, userId, item, extradata, limitedStack, limitedSells);
  }

  @Override
  public void onClick(final GameClient client, final Room room, Object[] objects) {
    Habbo habbo = client.getHabbo();
    RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(habbo);
    RoomTile location = room.getLayout().getTile(getX(), getY());
    if (!isAround(room, location, habbo)) {
      return;
    }
    if (!data.isJailed()) {
      habbo.whisper("Vous devez être en prison pour casser ce rocher.");
      return;
    }
    int effectId = getEffectId();
    if (effectId > -1 && habbo.getRoomUnit().getEffectId() != effectId) {
      habbo.whisper("Vous devez porter l'effet " + effectId + " pour casser ce rocher.");
      return;
    }

    int maxState = this.getBaseItem().getStateCount() - 1;
    int currentState = 0;
    try {
      currentState = Integer.parseInt(this.getExtradata());
    } catch (NumberFormatException ignored) {
    }

    Instant now = Instant.now();

    // If cooldown is active and not in progress, block
    if (finish != null && finish.isAfter(now) && currentState == 0) {
      long waitTime = (finish.toEpochMilli() - now.toEpochMilli()) / 1000;
      habbo.whisper(
          "Vous devez attendre encore " + waitTime + " seconde(s) avant de casser ce rocher.");
      return;
    }

    // If not in progress, start the sequence
    if (currentState == 0) {
      this.finish = now.plusSeconds(getDelaySeconds());
      updateState("1", room);
      habbo.whisper("Vous commencez à casser le rocher ! Continuez à cliquer avant la fin du temps imparti !");
      return;
    }

    // If in progress and within time
    if (finish == null || !finish.isAfter(now)) {
      // Time ran out, reset
      resetRock(room, now);
      habbo.whisper("Vous avez été trop lent ! Réessayez après le délai d'attente.");
      return;
    }

    if (currentState < maxState) {
      updateState(String.valueOf(currentState + 1), room);
      habbo.whisper("Continuez ! Progression : " + (currentState + 1) + "/" + maxState);
      return;
    }

    // Success: break the rock
    resetRock(room, now);
    room.sendComposer(new RoomUserShoutComposer(
        new RoomChatMessage("*Casse le rocher et réduit sa peine de prison*", habbo,
            habbo, RoomChatMessageBubbles.NORMAL)).compose());
    breakRock(habbo, data);
  }

  private void updateState(String currentState, Room room) {
    this.setExtradata(currentState);
    this.needsUpdate(true);
    room.updateItemState(this);
  }

  private void resetRock(Room room, Instant now) {
    updateState("0", room);
    this.finish = now.plusSeconds(getDelaySeconds());
  }

  private void breakRock(Habbo habbo, RpAvatar data) {
    RolePlay.getPrisonService().decreaseJailTime(habbo, data, getAmount());
  }

  private static int getAmount() {
    return Emulator.getConfig().getInt("features.jail.rock.decrease", 30);
  }

  private static int getDelaySeconds() {
    return Emulator.getConfig().getInt("features.jailrock.time", 15);
  }

  private static boolean isAround(Room room, RoomTile location, Habbo habbo) {
    return room.getLayout().getTilesAround(location, 0, false)
        .contains(habbo.getRoomUnit().getCurrentLocation());
  }

  private static int getEffectId() {
    return Emulator.getConfig().getInt("features.jailrock.effect", 30);
  }

  @Override
  public void onPickUp(Room room) {

  }
}
