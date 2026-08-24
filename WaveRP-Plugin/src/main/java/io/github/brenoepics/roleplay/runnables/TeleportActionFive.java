package io.github.brenoepics.roleplay.runnables;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomUnit;
import com.eu.habbo.habbohotel.users.HabboItem;

class TeleportActionFive implements Runnable {
		private final HabboItem currentTeleport;
		private final Room room;
		private final GameClient client;

		public TeleportActionFive(HabboItem currentTeleport, Room room, GameClient client) {
				this.currentTeleport = currentTeleport;
				this.client = client;
				this.room = room;
		}

		@Override
		public void run() {
				if (this.client.getHabbo().getHabboInfo().getCurrentRoom() != this.room)
						return;

				if (this.room.getLayout() == null || this.currentTeleport == null) return;

				this.room.updateItem(this.currentTeleport);
		}
}
