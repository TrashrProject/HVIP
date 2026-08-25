package io.github.brenoepics.roleplay.commands.generic;

import static io.github.brenoepics.roleplay.commands.generic.CommandsCounter.HELP_TIMEOUT;
import static io.github.brenoepics.roleplay.features.job.JobPermissions.POLICE_ALERT;

import com.eu.habbo.Emulator;
import com.eu.habbo.habbohotel.commands.Command;
import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.rooms.Room;
import com.eu.habbo.habbohotel.rooms.RoomChatMessageBubbles;
import com.eu.habbo.habbohotel.users.Habbo;
import com.eu.habbo.messages.outgoing.generic.alerts.BubbleAlertComposer;
import gnu.trove.map.hash.THashMap;
import io.github.brenoepics.roleplay.RolePlay;
import io.github.brenoepics.roleplay.features.user.RpAvatar;
import io.github.brenoepics.roleplay.utilities.types.Timeout;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class HelpCommand extends Command {

    private static final Logger LOGGER = LoggerFactory.getLogger(HelpCommand.class);

    public HelpCommand(String permission, String[] keys) {
        super(permission, keys);
    }

    @Override
    public boolean handle(GameClient gameClient, String[] params) {
        if (params.length != 1) {
            gameClient.getHabbo().whisper(":aide ou :911", RoomChatMessageBubbles.ALERT);
            return true;
        }

        Timeout timeout = RolePlay.getCommandsCounter().getCoolDown("help").getTimeOut(gameClient.getHabbo().getHabboInfo().getId());
        if (timeout != null) {
            gameClient.getHabbo().whisper("Vous devez attendre " + timeout.getFinish().minusMillis(System.currentTimeMillis()).getEpochSecond() + " seconde(s) avant de rappeler la police.");
            return true;
        }

        Room room = gameClient.getHabbo().getRoomUnit().getRoom();
        if (room == null) {
            gameClient.getHabbo().whisper("Impossible d'envoyer l'appel depuis cet endroit.", RoomChatMessageBubbles.ALERT);
            LOGGER.error("[NaHabbo RolePlay] {} tried to execute a command in hotel view", gameClient.getHabbo().getHabboInfo().getUsername());
            return true;
        }

        ConcurrentHashMap<Integer, Habbo> habbos = Emulator.getGameEnvironment().getHabboManager().getOnlineHabbos();
        List<Habbo> policeOfficers = new ArrayList<>();
        for (Habbo habbo : habbos.values()) {
            RpAvatar habboData = RolePlay.getAvatarManager().getRpAvatar(habbo);
            if (habboData.getJobRankEntity().hasPermission(POLICE_ALERT) && habboData.isDuty()) {
                policeOfficers.add(habbo);
            }
        }

        if (policeOfficers.isEmpty()) {
            gameClient.getHabbo().whisper("Aucun policier n'est actuellement en service.", RoomChatMessageBubbles.ALERT);
            return true;
        }

        //TODO: make this into livefeed
        policeOfficers.forEach(officer -> {
            officer.whisper(gameClient.getHabbo().getHabboInfo().getUsername() + " demande de l'aide dans " + room.getName() + " (" + room.getId() + ")", RoomChatMessageBubbles.RADIO);
            THashMap<String, String> notify_keys = new THashMap<>();
            notify_keys.put("display", "BUBBLE");
            notify_keys.put("image", "${image.library.url}notifications/mention.png");
            notify_keys.put("linkUrl", "event:navigator/goto/" + room.getId());
            notify_keys.put("message", gameClient.getHabbo().getHabboInfo().getUsername() + " demande de l'aide dans " + room.getName() + " (" + room.getId() + ")");
            officer.getClient().sendResponse(new BubbleAlertComposer("911call", notify_keys));
        });
        gameClient.getHabbo().whisper("Vous avez appel\u00e9 la police.", RoomChatMessageBubbles.ALERT);
        RolePlay.getCommandsCounter().getCoolDown("help").addTimeOut(gameClient.getHabbo().getHabboInfo().getId(), HELP_TIMEOUT);
        return true;
    }
}
