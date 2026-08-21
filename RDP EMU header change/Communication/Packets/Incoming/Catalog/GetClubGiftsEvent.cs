using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Catalog;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    class GetClubGiftsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            // ParadiseRP diagnostic: Nitro 2.1.1 currently throws a DataView
            // out-of-bounds error immediately after ClubGiftsMessageComposer
            // (server header 619). Suppress this optional response temporarily
            // so we can prove whether that packet is the parser-crash source.
            // Session.SendMessage(new ClubGiftsComposer());
        }
    }
}
