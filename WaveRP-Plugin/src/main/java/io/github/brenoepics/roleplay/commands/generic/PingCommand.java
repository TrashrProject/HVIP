package io.github.brenoepics.roleplay.commands.generic;

import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import io.netty.buffer.Unpooled;
import io.netty.channel.Channel;
import java.util.concurrent.TimeUnit;

/**
 * :ping - shows the user's ping/latency in a whisper bubble.
 */
public class PingCommand extends Command {

  public PingCommand(String permission, String[] keys) {
    super(permission, keys);
  }

  @Override
  public boolean handle(GameClient gameClient, String[] params) {
    if (params.length != 1) {
      gameClient.getHabbo().whisper(":ping", RoomChatMessageBubbles.ALERT);
      return true;
    }

    Channel ch = gameClient.getChannel();
    if (ch == null || !ch.isOpen()) {
      gameClient.getHabbo().whisper("Ping: connection closed", RoomChatMessageBubbles.ALERT);
      return true;
    }

    final long start = System.nanoTime();
    try {
      ch.writeAndFlush(Unpooled.EMPTY_BUFFER).addListener(future -> {
        long elapsedMs = TimeUnit.NANOSECONDS.toMillis(System.nanoTime() - start);
        // Ensure a minimum of 1ms for user feedback clarity
        if (elapsedMs < 0) elapsedMs = 0;
        String message = "Ping: " + elapsedMs + " ms";
        gameClient.getHabbo().whisper(message, RoomChatMessageBubbles.ALERT);
      });
    } catch (Throwable t) {
      gameClient.getHabbo().whisper("Ping: N/A", RoomChatMessageBubbles.ALERT);
    }
    return true;
  }
}
