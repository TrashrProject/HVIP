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
    class VolverAmostrar : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            GameClient requester = Session.GetHabbo().GuideOtherUser;
            requester.SendMessage(new MOTDNotificationComposer("Gracias por colaborar"));
            ServerPacket message = new ServerPacket(ServerPacketHeader.OnGuideSessionDetachedMessageComposer);

            /* guide - cerramos session  */
            message.WriteInteger(2);
            requester.SendMessage(message);

            /* user - cerramos session */
            ServerPacket message2 = new ServerPacket(ServerPacketHeader.OnGuideSessionDetachedMessageComposer);
            message.WriteInteger(0);
            Session.SendMessage(message2);

            /* user - detach session */
            ServerPacket message3 = new ServerPacket(ServerPacketHeader.OnGuideSessionDetachedMessageComposer);
            Session.SendMessage(message3);

            /* guide - detach session */
            ServerPacket message4 = new ServerPacket(ServerPacketHeader.OnGuideSessionDetachedMessageComposer);
            requester.SendMessage(message4);

            Console.WriteLine("The Close was Called");


            requester.GetHabbo().GuideOtherUser = null;
            Session.GetHabbo().GuideOtherUser = null;

            /*Guide - tool */
            if (Session.GetHabbo().Rank >= 4)
            {
                Session.SendMessage(new MostrarTool(Session));
            }
        }
    }
}
