using Plus.Communication.Packets.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class GuideSpeak : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var message = Packet.PopString();
            var requester = Session.GetHabbo().GuideOtherUser;
            var messageC = new ServerPacket(ServerPacketHeader.OnGuideSessionMsgMessageComposer);
            messageC.WriteString(message);
            messageC.WriteInteger(Session.GetHabbo().Id);
            requester.SendMessage(messageC);
            Session.SendMessage(messageC);
        }
    }
}
