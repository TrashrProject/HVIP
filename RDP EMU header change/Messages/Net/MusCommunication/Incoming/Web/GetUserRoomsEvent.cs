using Plus.Messages.Net.MusCommunication.Outgoing.Web;
using Plus.Net;
using System;

namespace Plus.Messages.Net.MusCommunication.Incoming.Web
{
    class GetUserRoomsEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            MUS.SendMessage(new SendUserRoomsComposer());
        }
    }
}
