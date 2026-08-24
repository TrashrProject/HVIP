package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.features.job.JobPermissions.POLICE_TAZE;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;

public class EquipCommand extends Command {

    public EquipCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(gameClient.getHabbo());
        if (data.isPassive()) {
            gameClient.getHabbo().whisper("You cannot execute RolePlay commands while passive mode is on!", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (params.length != 2) {
            gameClient.getHabbo().whisper(":equip <weapon>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        boolean isPolice = data.getJobRankEntity().hasPermission(POLICE_TAZE) && data.isDuty() && params[1].equalsIgnoreCase("tazor");
        RPItem item = isPolice ? RolePlay.getItemManager().getItemByName(params[1]) : data.getInventory().getSlotItem(params[1]);
        if (item == null) {
            gameClient.getHabbo().whisper("You do not have any item called " + params[1], RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (!item.getInteractionType().equals("weapon")) {
            gameClient.getHabbo().whisper("You cannot equip this item", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (item.getPermission() != null && !gameClient.getHabbo().getHabboInfo().getRank().hasPermission(item.getPermission(), false)) {
            gameClient.getHabbo().whisper("You do not have permission to equip this item", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (item.getEnableId() != -1) {
            gameClient.getHabbo().getHabboInfo().getCurrentRoom().giveEffect(gameClient.getHabbo(), item.getEnableId(), -1);
        }

        data.getInventory().equipWeapon(item);
        data.getInventory().updateInventory(gameClient.getHabbo());
        gameClient.getHabbo().whisper("You have equipped your " + item.getDisplayName(), RoomChatMessageBubbles.ALERT);
        return true;
    }
}
