using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    class TargetedOfferBuyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int priceCredits;
            int priceDiamonds;
            int id;

            ItemData furni = null;
            
                
            id = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["furni_1"]);


            priceCredits = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["cost_credits"]);

            priceDiamonds = Int32.Parse(PlusEnvironment.GetOffers().DBOffer["cost_diamonds"]);

            if (PlusEnvironment.GetGame().GetItemManager().GetItem(id, out furni))
                {
                    Item purchasefurni = ItemFactory.CreateSingleItemNullable(furni, Session.GetHabbo(), "", "");
                    if (purchasefurni != null && Session.GetHabbo().Credits > priceCredits && Session.GetHabbo().Diamonds > priceDiamonds)
                    {
                        if (priceCredits > 0)
                        {
                            Session.GetHabbo().Credits -= priceCredits;
                            Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                        }

                        if (priceDiamonds > 0)
                        {
                            Session.GetHabbo().Diamonds -= priceDiamonds;
                            Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Diamonds, 0, 5));
                        }

                        Session.GetHabbo().GetInventoryComponent().TryAddItem(purchasefurni);
                        Session.SendMessage(new FurniListNotificationComposer(purchasefurni.Id, 1));
                    Session.SendMessage(new FurniListUpdateComposer());
                    }
                }
            

                
        }
    }   
}
