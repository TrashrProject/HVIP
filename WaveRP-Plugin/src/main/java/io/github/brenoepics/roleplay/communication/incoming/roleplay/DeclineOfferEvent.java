package io.github.brenoepics.roleplay.communication.incoming.roleplay;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.features.offer.OfferManager;

public class DeclineOfferEvent extends IncomingWebMessage<DeclineOfferEvent.JSONDeclineOffer> {

    public DeclineOfferEvent() {
        super(DeclineOfferEvent.JSONDeclineOffer.class);
    }

    public final OfferManager offerManager = new OfferManager();

    @Override
    public void handle(GameClient client, DeclineOfferEvent.JSONDeclineOffer message) {
        Habbo habbo = client.getHabbo();
        if (habbo == null) return;

        offerManager.declineOffer(habbo.getHabboInfo().getId(), message.code);
    }

    static class JSONDeclineOffer {
        public boolean accept;
        public String code;
    }
}