package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.messages.ServerMessage;
import io.github.brenoepics.roleplay.communication.packets.emulator.outgoing.TargetLockComposer;
import io.github.brenoepics.roleplay.features.targetlock.TargetLockService;

public class TargetLockCommand extends Command {

  public TargetLockCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) throws Exception {
    if (!TargetLockService.hasClickedUser(gameClient.getHabbo())) {
      gameClient.getHabbo().whisper("Vous n'avez sélectionné aucune cible à verrouiller !");
      return true;
    }

    boolean isLocked = TargetLockService.switchLock(gameClient.getHabbo());
    String clickedUser = TargetLockService.getClickedUser(gameClient.getHabbo());

    ServerMessage composer = new TargetLockComposer(clickedUser, isLocked).compose();
    gameClient.sendResponse(composer);
    return true;
  }
}
