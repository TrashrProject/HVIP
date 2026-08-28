package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.HIT_TIMEOUT;
import static io.github.brenoepics.roleplay.features.user.HungerRunner.MISSING_ENERGY;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserShoutComposer;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import java.util.ArrayList;
import java.util.List;
import org.jetbrains.annotations.NotNull;

/**
 * :spit command - similar to :hit but always applies 1 damage.
 */
public class SpitCommand extends Command {

  public SpitCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    Habbo attacker = gameClient.getHabbo();
    RpAvatar attackerData = RolePlay.getAvatarManager().getRpAvatar(attacker);

    if (attackerData.isPassive()) {
      attacker.whisper("Vous ne pouvez pas utiliser cette commande en mode passif.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (params.length != 2) {
      attacker.whisper(":cracher <pseudo>", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!attackerData.hasEnergy()) {
      attacker.whisper(MISSING_ENERGY, RoomChatMessageBubbles.ALERT);
      return true;
    }

    Habbo target = attacker.getHabboInfo().getCurrentRoom().getHabbo(params[1]);
    if (target == null) {
      attacker.whisper("Le joueur " + params[1] + " est introuvable.", RoomChatMessageBubbles.ALERT);
      return true;
    }
    if (target == attacker) {
      attacker.whisper("Vous ne pouvez pas vous viser vous-m\u00eame.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    RpAvatar targetData = RolePlay.getAvatarManager().getRpAvatar(target);
    if (targetData.isPassive() && !targetData.isAggressive()) {
      attacker.whisper("Vous ne pouvez pas viser " + target.getHabboInfo().getUsername()
          + " car ce joueur est en mode passif.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (attackerData.isAtSafeZone() && (!targetData.isAggressive() || !attackerData.isAggressive())) {
      attacker.whisper("Vous ne pouvez pas utiliser cette commande dans une zone prot\u00e9g\u00e9e.",
          RoomChatMessageBubbles.ALERT);
      return true;
    }

    if (!Emulator.getConfig().getBoolean("features.organizations.friendly_fire")
        && attackerData.getOrganizationId() == targetData.getOrganizationId()) {
      attacker.whisper("Vous ne pouvez pas viser " + target.getHabboInfo().getUsername()
          + " car vous appartenez \u00e0 la m\u00eame organisation.", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("spit")
        .getTimeOut(attacker.getHabboInfo().getId());
    if (timeout != null) {
      attacker.whisper(
          "Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis())
              .getEpochSecond() + " seconde(s) avant de cracher de nouveau.");
      return true;
    }

    List<RoomTile> diag = getAround(target);
    List<RoomTile> nonDiag = getTilesAround(target);
    RoomTile middleTile = target.getRoomUnit().getCurrentLocation();

    List<RoomTile> allowedTiles = getRoomTiles(diag, nonDiag, middleTile, target);

    if (!isAttackAllowed(attacker, target, nonDiag, middleTile)) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(
          createActionMessage("* Crache sur " + target.getHabboInfo().getUsername() + ", mais le rate *",
              attacker).compose());
      RolePlay.getCommandsCounter().getCoolDown("spit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    if (!allowedTiles.contains(attacker.getRoomUnit().getCurrentLocation())) {
      attacker.getHabboInfo().getCurrentRoom().sendComposer(
          createActionMessage("* Crache sur " + target.getHabboInfo().getUsername() + ", mais le rate *",
              attacker).compose());
      RolePlay.getCommandsCounter().getCoolDown("spit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    if (targetData.isDead()) {
      attacker.whisper(target.getHabboInfo().getUsername() + " est d\u00e9j\u00e0 inconscient.",
          RoomChatMessageBubbles.ALERT);
      RolePlay.getCommandsCounter().getCoolDown("spit")
          .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
      return true;
    }

    int damage = 1; // fixed damage

    // apply damage (reuse HitCommand messaging for consistency)
    HitCommand.hitUser(attacker, targetData, damage, target);

    attackerData.getCombatStats().recordPunchThrown();
    attackerData.executeAction();
    RolePlay.getCommandsCounter().getCoolDown("spit")
        .addTimeOut(attacker.getHabboInfo().getId(), HIT_TIMEOUT);
    return true;
  }

  private static boolean isAttackAllowed(Habbo attacker, Habbo target, List<RoomTile> nonDiag,
      RoomTile middleTile) {
    RoomTile attackerTile = attacker.getRoomUnit().getCurrentLocation();
    if (attackerTile.equals(middleTile) || nonDiag.contains(attackerTile)) {
      return true;
    }
    boolean attackerWalking = attacker.getRoomUnit().isWalking();
    boolean targetWalking = target.getRoomUnit().isWalking();
    return attackerWalking && targetWalking && isAdjacentDiagonal(attackerTile, middleTile);
  }

  private static boolean isAdjacentDiagonal(RoomTile a, RoomTile b) {
    int dx = Math.abs(a.x - b.x);
    int dy = Math.abs(a.y - b.y);
    return dx == 1 && dy == 1;
  }

  private static List<RoomTile> getAround(Habbo habbo) {
    return habbo.getHabboInfo().getCurrentRoom().getLayout()
        .getTilesAround(habbo.getRoomUnit().getCurrentLocation(), 0, true);
  }

  private static List<RoomTile> getTilesAround(Habbo habbo) {
    return habbo.getHabboInfo().getCurrentRoom().getLayout()
        .getTilesAround(habbo.getRoomUnit().getCurrentLocation(), 0, false);
  }

  private static @NotNull List<RoomTile> getRoomTiles(List<RoomTile> tiles, List<RoomTile> nonDiag,
      RoomTile middleTile, Habbo habbo) {
    List<RoomTile> tilesCheck1 = new ArrayList<>();
    tilesCheck1.addAll(tiles);
    tilesCheck1.addAll(nonDiag);
    tiles.add(middleTile);
    tilesCheck1.add(middleTile);
    tilesCheck1.add(habbo.getRoomUnit().getStartLocation());
    tilesCheck1.add(habbo.getRoomUnit().getPreviousLocation());
    return tilesCheck1;
  }

  private static RoomUserShoutComposer createActionMessage(String message, Habbo actor) {
    return new RoomUserShoutComposer(new RoomChatMessage(message, actor, actor,
        RoomChatMessageBubbles.NORMAL));
  }
}
