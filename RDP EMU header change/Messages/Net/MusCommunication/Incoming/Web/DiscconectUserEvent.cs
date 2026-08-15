using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.Messages.Net.MusCommunication.Outgoing.Web;
using Plus.Net;
using System;

namespace Plus.Messages.Net.MusCommunication.Incoming.Web
{
    class DiscconectUserEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            // Desconectamos al usuario
            int UserID = 0;
            if (!int.TryParse(Packet.PacketData, out UserID))
                return;

            Habbo Habbo = PlusEnvironment.GetHabboById(UserID);
            if (Habbo == null)
                return;

            if (Habbo.GetClient() == null)
                return;

            Habbo.GetClient().Disconnect();
        }
    }
}
