using Plus.Communication.Packets.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class VisitRoom : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var requester = Session.GetHabbo().GuideOtherUser;
            var message = new ServerPacket(ServerPacketHeader.RoomForwardMessageComposer);
            message.WriteInteger(requester.GetHabbo().CurrentRoomId);
            Session.SendMessage(message);
        }
    }
}
