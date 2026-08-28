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
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.RPItem;
import java.sql.ResultSet;
import java.sql.SQLException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class DrugMachine extends InteractionDefault {

    private static final Logger LOGGER = LoggerFactory.getLogger(DrugMachine.class);
    private boolean occupied = false;
    private boolean crafted = false;

    public DrugMachine(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public DrugMachine(int id, int userId, Item item, String extradata, int limitedStack, int limitedSells) {
        super(id, userId, item, extradata, limitedStack, limitedSells);
    }

    @Override
    public void onClick(final GameClient client, final Room room, Object[] objects) {
        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(client.getHabbo());
        if (room.getCategory() != Emulator.getConfig().getInt("nahabbo.features.room.category")) {
            this.setExtradata(this.getExtradata().isEmpty() ? "0" : String.valueOf((Integer.parseInt(this.getExtradata()) + 1) % this.getBaseItem().getStateCount()));
            this.needsUpdate(true);
            room.updateItemState(this);
            return;
        }

        if (this.occupied || data.isDead()) {
            return;
        }

        Organization organization = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        if (organization == null) {
            client.getHabbo().whisper("Vous devez appartenir à une organisation pour utiliser cette machine.");
            return;
        }
        RPItem drugItem = RolePlay.getItemManager().getItemByName(this.getBaseItem().getCustomParams());
        if (drugItem == null) {
            LOGGER.error("Drug item {} not found for drug machine with id {} check the custom_params", this.getBaseItem().getCustomParams(), this.getId());
            return;
        }
        if (!drugItem.getCrafterOrganizations().contains(organization.getType())) {
            client.getHabbo().whisper("Votre organisation ne peut pas fabriquer cette drogue.");
            return;
        }
        if (this.crafted) {
            client.getHabbo().whisper("Cette machine a déjà été utilisée. Réessayez plus tard.");
            return;
        }

        DrugMachine machine = this;
        RoomTile location = room.getLayout().getTile(getX(), getY());
        if (!client.getHabbo().getHabboInfo().getCurrentRoom().getLayout().getTilesAround(location, 0, false).contains(client.getHabbo().getRoomUnit().getCurrentLocation()))
            return;

        this.occupied = true;
        room.updateItem(this);
        client.getHabbo().getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(new RoomChatMessage("*Commence à fabriquer de la drogue*", client.getHabbo(), client.getHabbo(), RoomChatMessageBubbles.NORMAL)).compose());
        Emulator.getThreading().run(() -> {
            if (client.getHabbo() == null || !client.getHabbo().getHabboInfo().getCurrentRoom().getLayout().getTilesAround(location, 0, false).contains(client.getHabbo().getRoomUnit().getCurrentLocation()))
                return;

            handleCraft(drugItem, client.getHabbo(), data);
            this.crafted = true;
            machine.setExtradata("1");
            room.updateItem(machine);
            Emulator.getThreading().run(() -> {
                machine.setExtradata("2");
                room.updateItem(machine);
                machine.crafted = false;
            }, Emulator.getConfig().getInt("nahabbo.features.drugmachine.cooldown") * 1000L);

            this.occupied = false;
        }, Emulator.getConfig().getInt("nahabbo.features.drugs.craft.time") * 1000L);
    }

    private void handleCraft(RPItem drugItem, Habbo habbo, RpAvatar data) {
        data.getInventory().addItem(habbo, drugItem, 1);
        habbo.getHabboInfo().getCurrentRoom().sendComposer(new RoomUserShoutComposer(new RoomChatMessage(Emulator.getTexts().getValue("features.drugmachine.craft." + drugItem.getDisplayName().toLowerCase() + ".message", "*Fabrique : %drug%*").replace("%user%", habbo.getHabboInfo().getUsername()).replace("%drug%", drugItem.getDisplayName()), habbo, habbo, RoomChatMessageBubbles.NORMAL)).compose());
    }

    @Override
    public void onPickUp(Room room) {
        this.occupied = false;
    }
}
