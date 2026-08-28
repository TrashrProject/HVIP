package io.github.brenoepics.roleplay.runnables;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.items.interactions.InteractionTeleport;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomUnitStatus;
import com.eu.habbo.habbohotel.users.HabboItem;
import com.eu.habbo.messages.outgoing.rooms.users.RoomUserStatusComposer;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import org.jetbrains.annotations.NotNull;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

class TeleportActionTwo implements Runnable {
		private static final Logger LOGGER = LoggerFactory.getLogger(TeleportActionTwo.class);

		private final HabboItem currentTeleport;
		private final Room room;
		private final GameClient client;

		public TeleportActionTwo(HabboItem currentTeleport, Room room, GameClient client) {
				this.currentTeleport = currentTeleport;
				this.client = client;
				this.room = room;
		}

		@Override
		public void run() {
				if (this.client.getHabbo().getHabboInfo().getCurrentRoom() != this.room)
						return;

				if (this.client.getHabbo().getRoomUnit().getGoal() != this.room.getLayout().getTile(this.currentTeleport.getX(), this.currentTeleport.getY())) {
						return;
				}

				this.room.sendComposer(new RoomUserStatusComposer(this.client.getHabbo().getRoomUnit()).compose());

			InteractionTeleport currentTeleport1 = getInteractionTeleport();

			this.room.updateItem(this.currentTeleport);

				if (currentTeleport1.getTargetRoomId() == 0) {
						Emulator.getThreading().run(new TeleportActionFive(this.currentTeleport, this.room, this.client), 0);
						return;
				}

				Emulator.getThreading().run(new TeleportActionThree(this.currentTeleport, this.room, this.client), 0);
		}

	private @NotNull InteractionTeleport getInteractionTeleport() {
		InteractionTeleport currentTeleport1 = (InteractionTeleport) this.currentTeleport;

		if (currentTeleport1.getTargetRoomId() > 0 && currentTeleport1.getTargetId() > 0) {
					HabboItem item = this.room.getHabboItem(currentTeleport1.getTargetId());
					if (item == null) {
							currentTeleport1.setTargetRoomId(0);
							currentTeleport1.setTargetId(0);
					} else if (((InteractionTeleport) item).getTargetRoomId() != currentTeleport1.getTargetRoomId()) {
							currentTeleport1.setTargetId(0);
							currentTeleport1.setTargetRoomId(0);
							((InteractionTeleport) item).setTargetId(0);
							((InteractionTeleport) item).setTargetRoomId(0);
					}
			} else {
					currentTeleport1.setTargetRoomId(0);
					currentTeleport1.setTargetId(0);
			}
		if (currentTeleport1.getTargetId() == 0) {
				try (Connection connection = Emulator.getDatabase().getDataSource().getConnection(); PreparedStatement statement = connection.prepareStatement("SELECT items_teleports.*, A.room_id as a_room_id, A.id as a_id, B.room_id as b_room_id, B.id as b_id FROM items_teleports INNER JOIN items AS A ON items_teleports.teleport_one_id = A.id INNER JOIN items AS B ON items_teleports.teleport_two_id = B.id  WHERE (teleport_one_id = ? OR teleport_two_id = ?)")) {
						statement.setInt(1, this.currentTeleport.getId());
						statement.setInt(2, this.currentTeleport.getId());

						try (ResultSet set = statement.executeQuery()) {
								if (set.next()) {
										if (set.getInt("a_id") != this.currentTeleport.getId()) {
												currentTeleport1.setTargetId(set.getInt("a_id"));
												currentTeleport1.setTargetRoomId(set.getInt("a_room_id"));
										} else {
												currentTeleport1.setTargetId(set.getInt("b_id"));
												currentTeleport1.setTargetRoomId(set.getInt("b_room_id"));
										}
								}
						}
				} catch (SQLException e) {
						LOGGER.error("Caught SQL exception", e);
				}
		}
		return currentTeleport1;
	}
}
