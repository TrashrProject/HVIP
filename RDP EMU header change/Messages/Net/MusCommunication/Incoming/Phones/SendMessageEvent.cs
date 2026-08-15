using Plus.HabboHotel.GameClients;
using Plus.Messages.Net.MusCommunication.Outgoing.Phones;
using Plus.Net;
using System;

namespace Plus.Messages.Net.MusCommunication.Incoming.Phones
{
    class SendMessageEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            string[] D = Packet.PacketData.Split('|');

            GameClient Client = null;
            if (PlusEnvironment.GetGame() != null && PlusEnvironment.GetGame().GetClientManager() != null)
                Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(D[1]));

            if (Client != null)
            {
                if (Client.GetPlay().TryGetCooldown("msg", true))
                {
                    Client.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                    return;
                }

                if (Client.GetPlay().Phone <= 0)
                    return;

                if (!Client.GetPlay().OwnedPhonesApps.ContainsKey(Convert.ToInt32(D[0])))
                    return;

                //PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "send_message," + D[2] + "|" + D[3]);

                MUS.SendMessage(new SendMessageComposer(Client, D[2], D[3]));

                Client.GetPlay().CooldownManager.CreateCooldown("msg", 1000, 3);
            }
        }
    }
}
