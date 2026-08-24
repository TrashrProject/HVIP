package io.github.brenoepics.roleplay.features.items.interactions;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.Item;
import com.eu.habbo.habbohotel.items.interactions.InteractionDefault;
import com.eu.habbo.habbohotel.rooms.Room;
import java.sql.ResultSet;
import java.sql.SQLException;

public class WardrobeCabin extends InteractionDefault {

		public WardrobeCabin(ResultSet set, Item baseItem) throws SQLException {
				super(set, baseItem);
		}

		public WardrobeCabin(int id, int userId, Item item, String extradata, int limitedStack, int limitedSells) {
				super(id, userId, item, extradata, limitedStack, limitedSells);
		}

		@Override
		public void onClick(final GameClient client, final Room room, Object[] objects) {

		}
}
