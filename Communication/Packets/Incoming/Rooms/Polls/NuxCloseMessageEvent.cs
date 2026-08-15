using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rewards.Rooms.Polls;

namespace Plus.Communication.Packets.Incoming.Rooms.Polls
{
    class NuxCloseMessageEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
           //here throw to parse
           if(Session.GetHabbo().isNoob)
            {
                NuxNoobWelcome.welcome(Session);
            }
        }
    }
}
