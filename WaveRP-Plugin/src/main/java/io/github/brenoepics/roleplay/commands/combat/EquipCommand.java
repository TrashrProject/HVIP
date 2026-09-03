package io.github.brenoepics.roleplay.commands.combat;

import static io.github.brenoepics.roleplay.features.job.JobPermissions.POLICE_TAZE;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.features.user.inventory.InventorySlot;
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
            gameClient.getHabbo().whisper(":equiper <arme, armure ou kit de réparation>", RoomChatMessageBubbles.ALERT);
            return true;
        }

        String itemName = String.join(" ", java.util.Arrays.copyOfRange(params, 1, params.length));
        boolean isPolice = data.getJobRankEntity().hasPermission(POLICE_TAZE) && data.isDuty() && itemName.equalsIgnoreCase("tazor");
        RPItem item = isPolice ? RolePlay.getItemManager().getItemByName(itemName) : data.getInventory().getSlotItem(itemName);
        if (item == null) {
            gameClient.getHabbo().whisper("Vous ne possédez aucun objet nommé " + itemName + ".", RoomChatMessageBubbles.ALERT);
            return true;
        }

        if ("repair".equalsIgnoreCase(item.getInteractionType())) {
            return useRepairKit(gameClient, data, item);
        }

        boolean isWeapon = item.getInteractionType().equals("weapon");
        boolean isArmor = item.getInteractionType().equals("shield");
        if (!isWeapon && !isArmor) {
            gameClient.getHabbo().whisper("Cet objet ne peut pas être équipé.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (item.getPermission() != null && !gameClient.getHabbo().getHabboInfo().getRank().hasPermission(item.getPermission(), false)) {
            gameClient.getHabbo().whisper("Vous n'avez pas la permission d'équiper cet objet.", RoomChatMessageBubbles.ALERT);
            return true;
        }
        if (isWeapon) {
            int visualEffect = RolePlay.getWeaponSkinService().getEquippedEffect(
                gameClient.getHabbo().getHabboInfo().getId(), item.getDisplayName(), item.getEnableId());
            if (visualEffect != -1) {
                gameClient.getHabbo().getHabboInfo().getCurrentRoom().giveEffect(gameClient.getHabbo(), visualEffect, -1);
            }
            if (!data.getInventory().equipWeapon(item)) {
                gameClient.getHabbo().whisper("Cette arme est inutilisable ou cassée.", RoomChatMessageBubbles.ALERT);
                return true;
            }
        } else {
            if (!data.getInventory().equipArmor(item)) {
                gameClient.getHabbo().whisper("Cette armure est inutilisable ou cassée.", RoomChatMessageBubbles.ALERT);
                return true;
            }

            InventorySlot armorSlot = data.getInventory().getSecondaryArmorSlot();
            data.setMaxShield(100);
            data.setShield(Math.max(0, Math.min(100, armorSlot.getDurability())));
            data.updateLife();
        }

        data.getInventory().updateInventory(gameClient.getHabbo());
        gameClient.getHabbo().whisper("Vous avez équipé " + item.getDisplayName() + ".", RoomChatMessageBubbles.ALERT);
        return true;
    }

    private boolean useRepairKit(GameClient gameClient, RpAvatar data, RPItem repairKit) {
        int repaired = 0;

        for (InventorySlot slot : data.getInventory().getAllSlots()) {
            repaired += repairSlot(slot);
        }
        for (InventorySlot slot : data.getInventory().getDepositBox().getAllSlots()) {
            repaired += repairSlot(slot);
        }

        if (repaired == 0) {
            gameClient.getHabbo().whisper("Aucune arme ou armure n'a besoin d'être réparée.",
                RoomChatMessageBubbles.ALERT);
            return true;
        }

        data.getInventory().removeItem(repairKit, 1);

        InventorySlot armorSlot = data.getInventory().getSecondaryArmorSlot();
        if (!armorSlot.isEmpty() && armorSlot.getItem() != null
            && "shield".equalsIgnoreCase(armorSlot.getItem().getInteractionType())) {
            data.setMaxShield(100);
            data.setShield(armorSlot.getDurability());
            data.updateLife();
        }

        data.getInventory().updateInventory(gameClient.getHabbo());
        gameClient.getHabbo().whisper("Kit de réparation utilisé : " + repaired
            + " équipement(s) restauré(s) à 100%.", RoomChatMessageBubbles.ALERT);
        return true;
    }

    private int repairSlot(InventorySlot slot) {
        if (slot == null || slot.isEmpty() || slot.getItem() == null || slot.getDurability() >= 100) {
            return 0;
        }
        String interactionType = slot.getItem().getInteractionType();
        if (!"weapon".equalsIgnoreCase(interactionType) && !"shield".equalsIgnoreCase(interactionType)) {
            return 0;
        }
        slot.repair();
        return 1;
    }
}
