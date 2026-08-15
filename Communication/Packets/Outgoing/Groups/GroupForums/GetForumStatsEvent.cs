using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class GetForumStatsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
                return;

            if (!group.ForumEnabled || group.GetForum() == null)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.group.not_found"));
                return;
            }

            session.SendMessage(new ForumDataComposer(group, session.GetHabbo().Id));
        }
    }
}