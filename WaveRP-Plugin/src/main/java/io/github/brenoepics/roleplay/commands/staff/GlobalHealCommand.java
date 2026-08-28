package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.LiveFeed;
import io.github.brenoepics.roleplay.utilities.template.PassiveTemplates;

public class GlobalHealCommand extends Command {

  public GlobalHealCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] strings) throws Exception {

    Habbo healer = gameClient.getHabbo();

    String msg = PassiveTemplates.HEAL.format(healer.getHabboInfo().getUsername(), "tout le monde");
    LiveFeed.sendGlobalAlert(LiveFeed.alert(msg));

    for (Habbo habbo : Emulator.getGameEnvironment().getHabboManager().getOnlineHabbos().values()) {
      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (avatar != null) {
        avatar.heal();
        avatar.resetHungry();
      }
    }

    return true;
  }
}
