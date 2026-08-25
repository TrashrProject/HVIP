package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomTile;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.organizations.Organization;
import io.github.brenoepics.roleplay.features.organizations.territories.Territory;
import io.github.brenoepics.roleplay.features.organizations.territories.WarException;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import java.sql.ResultSet;
import java.sql.SQLException;

public class ClaimTerritoryItem extends InteractionDefault {
    public ClaimTerritoryItem(ResultSet set, Item baseItem) throws SQLException {
        super(set, baseItem);
    }

    public ClaimTerritoryItem(int id, int userId, Item item, String extraData, int limitedStack, int limitedSells) {
        super(id, userId, item, extraData, limitedStack, limitedSells);
    }

    @Override
    public boolean isUsable() {
        return true;
    }

    @Override
    public void onClick(final GameClient client, final Room room, Object[] objects) {
        RoomTile location = room.getLayout().getTile(getX(), getY());
        if (!client.getHabbo().getHabboInfo().getCurrentRoom().getLayout().getTilesAround(location, 0, false).contains(client.getHabbo().getRoomUnit().getCurrentLocation()))
            return;

        RpAvatar data = RolePlay.getAvatarManager().getRpAvatar(client.getHabbo());
        if (data.isPassive()) {
            client.getHabbo().whisper("Vous ne pouvez pas effectuer cette action en mode passif !", RoomChatMessageBubbles.ALERT);
            return;
        }
        Territory territory = RolePlay.getOrganizationManager().getOrganizationTerritories().get(room.getId());
        Organization userOrg = RolePlay.getOrganizationManager().getOrganization(data.getOrganizationId());
        RoomUnit unit = client.getHabbo().getRoomUnit();
        if (unit == null || !check(territory, userOrg, room, client)) {
            return;
        }


        Timeout timeout = territory.getClaimCountDown().getTimeOut(0);
        if (timeout != null) {
            client.getHabbo().whisper("Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " secondes avant de revendiquer ce territoire !", RoomChatMessageBubbles.ALERT);
            return;
        }

        try {
            RolePlay.getOrganizationManager().startTerritoryWar(territory, userOrg);
        } catch (WarException e) {
            client.getHabbo().whisper(e.getMessage(), RoomChatMessageBubbles.ALERT);
        }
    }

    private boolean check(Territory territory, Organization userOrg, Room room, GameClient client) {
        if (userOrg == null || territory == null) {
            return false;
        }

        if (territory.getType() != userOrg.getType()) {
            client.getHabbo().whisper("Vous ne pouvez pas revendiquer ce territoire !", RoomChatMessageBubbles.ALERT);
            return false;
        }

        if (territory.isClaimedBy(userOrg.getId())) {
            client.getHabbo().whisper("Votre organisation possède déjà ce territoire !", RoomChatMessageBubbles.ALERT);
            return false;
        }

        if (RolePlay.getOrganizationManager().getOrganizationWars().containsKey(room.getId())) {
            client.getHabbo().whisper("Ce territoire est déjà attaqué !", RoomChatMessageBubbles.ALERT);
            return false;
        }

        if (territory.getClaimCountDown().getTimeOut(0) != null) {
            client.getHabbo().whisper("Vous devez attendre " + territory.getClaimCountDown().getTimeOut(0).getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " secondes avant de revendiquer ce territoire !", RoomChatMessageBubbles.ALERT);
            return false;
        }

        return true;
    }
}
