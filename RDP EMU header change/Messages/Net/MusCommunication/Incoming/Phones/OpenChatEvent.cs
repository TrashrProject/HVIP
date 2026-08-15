using Plus.HabboHotel.GameClients;
using Plus.Messages.Net.MusCommunication.Outgoing.Phones;
using Plus.Net;
using System;

namespace Plus.Messages.Net.MusCommunication.Incoming.Phones
{
    class OpenChatEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            string[] D = Packet.PacketData.Split('|');
            MUS.SendMessage(new OpenChatComposer(Convert.ToInt32(D[0]), Convert.ToInt32(D[1]), Convert.ToString(D[2])));
        }
    }
}
