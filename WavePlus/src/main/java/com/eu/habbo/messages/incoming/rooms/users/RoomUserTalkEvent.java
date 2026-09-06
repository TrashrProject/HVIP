package com.eu.habbo.messages.incoming.rooms.users;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.modtool.ScripterManager;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessage;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.rooms.RoomChatType;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.incoming.MessageHandler;
import com.eu.habbo.plugin.events.users.UserTalkEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;

public class RoomUserTalkEvent extends MessageHandler {
    private static final Logger LOGGER = LoggerFactory.getLogger(RoomUserTalkEvent.class);

    @Override
    public void handle() throws Exception {
        Room room = this.client.getHabbo().getHabboInfo().getCurrentRoom();
        if (room == null)
            return;

        if (!this.client.getHabbo().getHabboStats().allowTalk())
            return;

        RoomChatMessage message = new RoomChatMessage(this);

        if (message.getMessage().length() <= RoomChatMessage.MAXIMUM_LENGTH) {
            if (Emulator.getPluginManager().fireEvent(new UserTalkEvent(this.client.getHabbo(), message, RoomChatType.TALK)).isCancelled()) {
                return;
            }

            // During an accepted ParadisePhone call, normal room chat becomes a private
            // phone whisper automatically. Commands keep their normal behaviour.
            if (this.routeAcceptedPhoneCall(message)) {
                return;
            }

            room.talk(this.client.getHabbo(), message, RoomChatType.TALK);

            if (!message.isCommand) {
                if (RoomChatMessage.SAVE_ROOM_CHATS) {
                    Emulator.getThreading().run(message);
                }
            }
        } else {
            String reportMessage = Emulator.getTexts().getValue("scripter.warning.chat.length").replace("%username%", this.client.getHabbo().getHabboInfo().getUsername()).replace("%length%", message.getMessage().length() + "");
            ScripterManager.scripterDetected(this.client, reportMessage);
            LOGGER.info(reportMessage);
        }
    }

    private boolean routeAcceptedPhoneCall(RoomChatMessage message) {
        String text = message.getMessage();
        if (text == null || text.isBlank() || text.startsWith(":")) {
            return false;
        }

        Habbo sender = this.client.getHabbo();
        int senderId = sender.getHabboInfo().getId();
        int callId = 0;
        int peerId = 0;

        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement statement = connection.prepareStatement(
                     "SELECT id,caller_id,callee_id FROM phone_calls " +
                             "WHERE status='accepted' AND (caller_id=? OR callee_id=?) " +
                             "ORDER BY id DESC LIMIT 1")) {
            statement.setInt(1, senderId);
            statement.setInt(2, senderId);

            try (ResultSet result = statement.executeQuery()) {
                if (!result.next()) {
                    return false;
                }

                callId = result.getInt("id");
                int callerId = result.getInt("caller_id");
                int calleeId = result.getInt("callee_id");
                peerId = callerId == senderId ? calleeId : callerId;
            }
        } catch (Exception exception) {
            LOGGER.warn("Unable to resolve active ParadisePhone call for user {}", senderId, exception);
            return false;
        }

        Habbo peer = Emulator.getGameEnvironment().getHabboManager().getHabbo(peerId);
        if (peer == null || peer.getClient() == null || peer.getHabboInfo().getCurrentRoom() == null) {
            sender.whisper("Votre correspondant n’est plus disponible.", RoomChatMessageBubbles.NORMAL);
            return true;
        }

        String senderName = sender.getHabboInfo().getUsername();
        String peerName = peer.getHabboInfo().getUsername();
        RoomChatMessageBubbles bubble = message.getBubble();

        // Only the two participants receive these whisper packets. This also works
        // when they are standing in two different rooms.
        sender.whisper("À " + peerName + " : " + text, bubble);
        peer.whisper(senderName + " : " + text, bubble);

        // Keep the accepted call fresh while the players are actively talking.
        try (Connection connection = Emulator.getDatabase().getDataSource().getConnection();
             PreparedStatement statement = connection.prepareStatement(
                     "UPDATE phone_calls SET updated_at=? WHERE id=? AND status='accepted'")) {
            statement.setInt(1, Emulator.getIntUnixTimestamp());
            statement.setInt(2, callId);
            statement.executeUpdate();
        } catch (Exception exception) {
            LOGGER.debug("Unable to refresh ParadisePhone call {}", callId, exception);
        }

        return true;
    }
}
