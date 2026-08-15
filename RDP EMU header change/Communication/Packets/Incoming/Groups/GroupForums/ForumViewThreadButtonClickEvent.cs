using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups.GroupForums
{
    class ForumViewThreadButtonClickEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var groupcount = Packet.PopInt();
            //int group id
            //int forum post count
            //bool true
            //int group id
            //int forum post count
            //bool true
        }
    }
}
