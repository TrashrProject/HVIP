package io.github.brenoepics.roleplay.communication.incoming.roleplay;

import com.eu.habbo.habbohotel.gameclients.GameClient;
import com.eu.habbo.habbohotel.users.Habbo;
import io.github.brenoepics.roleplay.communication.incoming.IncomingWebMessage;
import io.github.brenoepics.roleplay.features.offer.OfferManager;

public class AcceptOfferEvent extends IncomingWebMessage<AcceptOfferEvent.JSONAcceptOffer> {

    public AcceptOfferEvent() {
        super(AcceptOfferEvent.JSONAcceptOffer.class);
    }

    public final OfferManager offerManager = new OfferManager();

    @Override
    public void handle(GameClient client, AcceptOfferEvent.JSONAcceptOffer message) {
        Habbo habbo = client.getHabbo();
        if (habbo == null)
            return;

        offerManager.acceptOffer(habbo, message.code);
    }

    static class JSONAcceptOffer {
        public boolean accept;
        public String code;
    }
}