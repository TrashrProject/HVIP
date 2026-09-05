package com.eu.habbo.messages.outgoing.rooms.items;

import com.eu.habbo.habbohotel.items.interactions.*;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.ServerMessage;
import com.eu.habbo.messages.outgoing.MessageComposer;
import com.eu.habbo.messages.outgoing.Outgoing;
import gnu.trove.iterator.TIntObjectIterator;
import gnu.trove.map.TIntObjectMap;
import gnu.trove.set.hash.THashSet;

import java.util.Locale;
import java.util.NoSuchElementException;

public class RoomFloorItemsComposer extends MessageComposer {
    private final TIntObjectMap<String> furniOwnerNames;
    private final THashSet<? extends HabboItem> items;

    public RoomFloorItemsComposer(TIntObjectMap<String> furniOwnerNames, THashSet<? extends HabboItem> items) {
        this.furniOwnerNames = furniOwnerNames;
        this.items = items;
    }

    @Override
    protected ServerMessage composeInternal() {
        this.response.init(Outgoing.RoomFloorItemsComposer);

        TIntObjectIterator<String> iterator = this.furniOwnerNames.iterator();

        this.response.appendInt(this.furniOwnerNames.size());
        for (int i = this.furniOwnerNames.size(); i-- > 0; ) {
            try {
                iterator.advance();
                this.response.appendInt(iterator.key());
                this.response.appendString(iterator.value());
            } catch (NoSuchElementException e) {
                break;
            }
        }

        this.response.appendInt(this.items.size());

        for (HabboItem item : this.items) {
            item.serializeFloorData(this.response);
            this.response.appendInt(item instanceof InteractionGift ? ((((InteractionGift) item).getColorId() * 1000) + ((InteractionGift) item).getRibbonId()) : (item instanceof InteractionMusicDisc ? ((InteractionMusicDisc) item).getSongId() : 1));
            item.serializeExtradata(this.response);
            this.response.appendInt(-1);
            this.response.appendInt(item instanceof InteractionTeleport || item instanceof InteractionSwitch || item instanceof InteractionSwitchRemoteControl || item instanceof InteractionVendingMachine || item instanceof InteractionInformationTerminal || item instanceof InteractionPostIt || item instanceof InteractionPuzzleBox ? 2 : (item.isUsable() || isParadiseTrash(item)) ? 1 : 0);
            this.response.appendInt(item.getUserId());
        }
        return this.response;
    }

    /**
     * ParadiseRP trash furniture must be advertised as usable to Nitro even when the item was
     * instantiated before the WaveRP plugin registered its custom interaction. Otherwise Nitro
     * only opens the furni information window and never sends ToggleFloorItemEvent.
     */
    private static boolean isParadiseTrash(HabboItem item) {
        if (item == null || item.getBaseItem() == null) {
            return false;
        }

        // Verified production base item for the classic Urban Trash Can / "Poubelle".
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

    public TIntObjectMap<String> getFurniOwnerNames() {
        return furniOwnerNames;
    }

    public THashSet<? extends HabboItem> getItems() {
        return items;
    }
}
