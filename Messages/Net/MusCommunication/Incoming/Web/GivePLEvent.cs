using Plus.Messages.Net.MusCommunication.Outgoing.Web;
using Plus.Net;
using System;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;

namespace Plus.Messages.Net.MusCommunication.Incoming.Web
{
    class GivePLEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            string[] D = Packet.PacketData.Split('|');

            int Amount = 0, UserID = 0;

            if (!int.TryParse(D[0], out Amount))
                return;

            if (!int.TryParse(D[1], out UserID))
                return;

            Habbo Habbo = PlusEnvironment.GetHabboById(UserID);
            if (Habbo == null)
                return;

            if (Habbo.GetClient() == null)
                return;

            Habbo.Diamonds += Amount;
            Habbo.GetClient().GetPlay().PLEarned += Amount;
            Habbo.GetClient().SendMessage(new HabboActivityPointNotificationComposer(Habbo.Diamonds, Amount, 5));

            Habbo.GetClient().SendNotification("¡Has recibido " + Amount.ToString() + " platino(s)!");
        }
    }
}
