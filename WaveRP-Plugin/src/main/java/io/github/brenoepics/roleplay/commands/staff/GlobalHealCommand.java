package io.github.brenoepics.roleplay.commands.staff;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
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

    Habbo staff = gameClient.getHabbo();
    int revived = 0;

    String msg = PassiveTemplates.HEAL.format(staff.getHabboInfo().getUsername(), "tout le monde");
    LiveFeed.sendGlobalAlert(LiveFeed.alert(msg));

    for (Habbo habbo : Emulator.getGameEnvironment().getHabboManager().getOnlineHabbos().values()) {
      RpAvatar avatar = RolePlay.getAvatarManager().getRpAvatar(habbo);
      if (avatar != null) {
        if (habbo.getRoomUnit() != null) {
          avatar.heal();
        } else {
          avatar.setHealth(avatar.getMaxHealth());
          avatar.setDead(false);
        }
        RolePlay.getHospitalService().finishHealing(habbo);
        avatar.updateDatabase();
        habbo.whisper("Le staff a remis votre vie au maximum.", RoomChatMessageBubbles.NORMAL);
        revived++;
      }
    }

    staff.whisper("Vie restaurée pour " + revived + " joueur(s) connecté(s).",
        RoomChatMessageBubbles.NORMAL);

    return true;
  }
}
