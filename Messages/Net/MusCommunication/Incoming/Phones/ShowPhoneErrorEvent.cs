using Plus.HabboHotel.GameClients;
using Plus.Messages.Net.MusCommunication.Outgoing.Phones;
using Plus.Net;
using System;

namespace Plus.Messages.Net.MusCommunication.Incoming.Phones
{
    class ShowPhoneErrorEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            string[] D = Packet.PacketData.Split('|');

            GameClient Client = null;
            if (PlusEnvironment.GetGame() != null && PlusEnvironment.GetGame().GetClientManager() != null)
                Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(D[1]));

            if (Client != null)
            {
                if (Client.GetPlay().Phone <= 0)
                    return;

                if (!Client.GetPlay().OwnedPhonesApps.ContainsKey(Convert.ToInt32(D[0])))
                    return;

                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "in_app_error," + D[2] + "|");
            }
        }
    }
}
