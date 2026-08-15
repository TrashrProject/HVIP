using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class AnswerGuideRequest : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            bool state = Packet.PopBoolean();

            if (!state)
                return;

            GameClient requester = Session.GetHabbo().GuideOtherUser;
            ServerPacket message = new ServerPacket(ServerPacketHeader.OnGuideSessionStartedMessageComposer);

            message.WriteInteger(requester.GetHabbo().Id);
            message.WriteString(requester.GetHabbo().Username);
            message.WriteString(requester.GetHabbo().Look);
            message.WriteInteger(Session.GetHabbo().Id);
            message.WriteString(Session.GetHabbo().Username);
            message.WriteString(Session.GetHabbo().Look);
            requester.SendMessage(message);
            Session.SendMessage(message);
        }
    }
}
