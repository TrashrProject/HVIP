package com.eu.habbo.messages.outgoing.rooms.items;

import com.eu.habbo.habbohotel.items.interactions.*;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.eu.habbo.messages.outgoing.Outgoing;

import java.util.Locale;

public class AddFloorItemComposer extends MessageComposer {
    private final HabboItem item;
    private final String itemOwnerName;

    public AddFloorItemComposer(HabboItem item, String itemOwnerName) {
        this.item = item;
        this.itemOwnerName = itemOwnerName == null ? "" : itemOwnerName;
    }

    @Override
    protected ServerMessage composeInternal() {
        this.response.init(Outgoing.AddFloorItemComposer);
        this.item.serializeFloorData(this.response);
        this.response.appendInt(this.item instanceof InteractionGift ? ((((InteractionGift) this.item).getColorId() * 1000) + ((InteractionGift) this.item).getRibbonId()) : (this.item instanceof InteractionMusicDisc ? ((InteractionMusicDisc) this.item).getSongId() : 1));
        this.item.serializeExtradata(this.response);
        this.response.appendInt(-1);
        this.response.appendInt(this.item instanceof InteractionTeleport || this.item instanceof InteractionSwitch || this.item instanceof InteractionSwitchRemoteControl || this.item instanceof InteractionVendingMachine || this.item instanceof InteractionInformationTerminal || this.item instanceof InteractionPostIt || this.item instanceof InteractionPuzzleBox ? 2 : (this.item.isUsable() || isParadiseTrash(this.item)) ? 1 : 0);
        this.response.appendInt(this.item.getUserId());
        this.response.appendString(this.itemOwnerName);
        return this.response;
    }

    /**
     * Same protection as RoomFloorItemsComposer for furniture placed while a room is already open.
     * Nitro must receive usage policy 1 for trash cans so a double-click sends ToggleFloorItemEvent.
     */
    private static boolean isParadiseTrash(HabboItem item) {
        if (item == null || item.getBaseItem() == null) {
            return false;
        }

        if (item.getBaseItem().getId() == 3266) {
            return true;
        }

        String internalName = item.getBaseItem().getName();
        String publicName = item.getBaseItem().getFullName();
        return matchesTrashName(internalName) || matchesTrashName(publicName);
    }

    private static boolean matchesTrashName(String value) {
        if (value == null || value.isBlank()) {
            return false;
        }

        String name = value.trim().toLowerCase(Locale.ROOT);
        return name.contains("trash")
            || name.contains("garbage")
            || name.contains("dumpster")
            || name.contains("dustbin")
            || name.contains("rubbish")
            || name.contains("waste")
            || name.contains("recycle")
            || name.contains("poubelle")
            || name.contains("corbeille")
            || name.contains("ordure")
            || name.contains("dechet")
            || name.equals("bin")
            || name.startsWith("bin_")
            || name.matches("^bin[0-9]+.*")
            || name.endsWith("_bin")
            || name.contains("_bin_");
    }

    public HabboItem getItem() {
        return item;
    }

    public String getItemOwnerName() {
        return itemOwnerName;
    }
}
