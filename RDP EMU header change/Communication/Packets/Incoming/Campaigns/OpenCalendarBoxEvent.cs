using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.LandingView;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.LandingView
{
    class OpenCalendarBoxEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            string eventName = Packet.PopString();
            int giftDay = Packet.PopInt();

            HabboStats habboStats = Session?.GetHabbo()?.GetStats();

            int currentDay = DateTime.Now.Day - 1;

            if (habboStats == null ||
                habboStats.openedGifts.Contains(giftDay) || giftDay < (currentDay - 2) ||
                giftDay > currentDay || eventName != "xmas16")
            {
                return;
            }

            Item newItem = null;
            if (!PlusEnvironment.GetGame().GetLandingManager().GenerateCalendarItem(
                Session.GetHabbo(), eventName, giftDay, out newItem))
            {
                return;
            }

            habboStats.addOpenedGift(giftDay);
            Session.GetHabbo().GetInventoryComponent().TryAddItem(newItem);
            Session.SendMessage(new FurniListUpdateComposer());
            Session.SendMessage(new CampaignCalendarGiftComposer(newItem.Data.ItemName));
        }
    }
}