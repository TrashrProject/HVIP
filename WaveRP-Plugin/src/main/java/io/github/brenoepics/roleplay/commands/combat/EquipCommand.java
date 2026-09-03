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
            gameClient.getHabbo().whisper("Vous ne pouvez pas utiliser les commandes RP en mode passif.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (params.length < 2) {
            gameClient.getHabbo().whisper(":equiper <arme ou armure>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        String itemName = String.join(" ", java.util.Arrays.copyOfRange(params, 1, params.length));
        boolean isPolice = data.getJobRankEntity().hasPermission(POLICE_TAZE) && data.isDuty() && itemName.equalsIgnoreCase("tazor");
        RPItem item = isPolice ? RolePlay.getItemManager().getItemByName(itemName) : data.getInventory().getSlotItem(itemName);
        if (item == null) {
            gameClient.getHabbo().whisper("Vous ne poss\u00e9dez aucun objet nomm\u00e9 " + itemName + ".", RoomChatMessageBubbles.ALERT);
            return true;
        }
        boolean isWeapon = item.getInteractionType().equals("weapon");
        boolean isArmor = item.getInteractionType().equals("shield");
        if (!isWeapon && !isArmor) {
            gameClient.getHabbo().whisper("Cet objet ne peut pas \u00eatre \u00e9quip\u00e9.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (item.getPermission() != null && !gameClient.getHabbo().getHabboInfo().getRank().hasPermission(item.getPermission(), false)) {
            gameClient.getHabbo().whisper("Vous n'avez pas la permission d'\u00e9quiper cet objet.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (isWeapon) {
            int visualEffect = RolePlay.getWeaponSkinService().getEquippedEffect(
                gameClient.getHabbo().getHabboInfo().getId(), item.getDisplayName(), item.getEnableId());
            if (visualEffect != -1) {
                gameClient.getHabbo().getHabboInfo().getCurrentRoom().giveEffect(gameClient.getHabbo(), visualEffect, -1);
            }
            data.getInventory().equipWeapon(item);
        } else {
            data.getInventory().equipArmor(item);
        }

        data.getInventory().updateInventory(gameClient.getHabbo());
        gameClient.getHabbo().whisper("Vous avez \u00e9quip\u00e9 " + item.getDisplayName() + ".", RoomChatMessageBubbles.ALERT);
        return true;
    }
}
