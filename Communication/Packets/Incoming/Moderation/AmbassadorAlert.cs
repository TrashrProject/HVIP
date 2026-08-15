using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Moderation
{
    class AmbassadorAlert : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().GetPermissions().HasRight("ambassador_tool") )
                return;
            int userId = Packet.PopInt();
            GameClient user = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(userId);
            if (user == null) return;
            user.SendMessage(new SuperNotificationComposer("", "${notification.ambassador.alert.warning.title}", "${notification.ambassador.alert.warning.message}", "", ""));
        }
    }
}