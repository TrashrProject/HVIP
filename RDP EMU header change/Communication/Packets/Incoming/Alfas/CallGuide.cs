using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class CallGuide : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Packet.PopBoolean();

            int userId = Packet.PopInt();
            string message = Packet.PopString();

            GuideManager guideManager = PlusEnvironment.GetGame().GetGuideManager();

            if (guideManager.GuidesCount <= 0)
            {
                var Response = new ServerPacket(ServerPacketHeader.OnGuideSessionError);
                Response.WriteInteger(0);
                Session.SendMessage(Response);
                return;
            }

            GameClient guide = guideManager.GetRandomGuide();

            if (guide == null)
            {
                var Response = new ServerPacket(ServerPacketHeader.OnGuideSessionError);
                Response.WriteInteger(0);
                Session.SendMessage(Response);
                return;
            }

            ServerPacket onGuideSessionAttached = new ServerPacket(ServerPacketHeader.OnGuideSessionAttachedMessageComposer);
            onGuideSessionAttached.WriteBoolean(false);
            onGuideSessionAttached.WriteInteger(userId);
            onGuideSessionAttached.WriteString(message);
            onGuideSessionAttached.WriteInteger(30);
            Session.SendMessage(onGuideSessionAttached);

            ServerPacket onGuideSessionAttached2 = new ServerPacket(ServerPacketHeader.OnGuideSessionAttachedMessageComposer);
            onGuideSessionAttached2.WriteBoolean(true);
            onGuideSessionAttached2.WriteInteger(userId);
            onGuideSessionAttached2.WriteString(message);
            onGuideSessionAttached2.WriteInteger(15);
            guide.SendMessage(onGuideSessionAttached2);
            guide.GetHabbo().GuideOtherUser = Session;
            Session.GetHabbo().GuideOtherUser = guide;

            //Console.WriteLine("hemos pedido ayuda a un alfa");
        }
    }
}
