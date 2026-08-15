using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Alfas;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class CloseGuideRequest : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var requester = Session.GetHabbo().GuideOtherUser;
            var message = new ServerPacket(ServerPacketHeader.GuideSessionEndedMessageComposer);
            message.WriteInteger(2);
            //requester.SendMessage(new MOTDNotificationComposer("Gracias por colaborar"));

            Session.SendMessage(message);
            requester.SendMessage(message);
        }
    }
}
