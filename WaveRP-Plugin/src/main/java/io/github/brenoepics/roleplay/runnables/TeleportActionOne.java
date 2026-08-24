package io.github.brenoepics.roleplay.runnables;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.users.HabboItem;

public class TeleportActionOne implements Runnable {
		private final HabboItem currentTeleport;
		private final Room room;
		private final GameClient client;

		public TeleportActionOne(HabboItem currentTeleport, Room room, GameClient client) {
				this.currentTeleport = currentTeleport;
				this.client = client;
				this.room = room;
		}

		public void run() {

				if (this.client.getHabbo().getHabboInfo().getCurrentRoom() != this.room) {
						return;
				}

				if (this.client.getHabbo().getRoomUnit().getGoal() != this.room.getLayout().getTile(this.currentTeleport.getX(), this.currentTeleport.getY())) {
						return;
				}

				int delay = 500;
				Emulator.getThreading().run(new TeleportActionTwo(this.currentTeleport, this.room, this.client),
						delay);
		}
}
