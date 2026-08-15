using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class BullyRequestAccept : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            bool accepted = Packet.PopBoolean();
        }
    }
}
