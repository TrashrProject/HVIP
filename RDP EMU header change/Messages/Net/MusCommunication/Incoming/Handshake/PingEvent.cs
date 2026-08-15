using Plus.Messages.Net.MusCommunication.Outgoing.Handshake;
using Plus.Net;
using System;

namespace Plus.Messages.Net.MusCommunication.Incoming.Handshake
{
    class PingEvent : IMusPacketEvent
    {
        public void Parse(MusConnection MUS, MusPacketEvent Packet)
        {
            MUS.SendMessage(new PongComposer());
        }
    }
}
