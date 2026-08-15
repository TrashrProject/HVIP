using Plus.Communication.Packets.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class CancelCallGuide : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var OnguideError = new ServerPacket(ServerPacketHeader.OnGuideSessionDetachedMessageComposer);
            Session.SendMessage(OnguideError);
        }
    }
}
