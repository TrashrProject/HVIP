using System.Linq;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Groups.Forums;

namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class GetThreadsListDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int startIndex = packet.PopInt();
            int unknown = packet.PopInt();//Start index

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
                return;

            if (!group.ForumEnabled || group.GetForum() == null)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.group.not_found"));
                return;
            }

            List<GroupThread> threads = group.GetForum().GetThreads().OrderByDescending(x => x.Pinned).ThenByDescending(x => x.LastReplierDate).ToList();

            session.SendMessage(new ThreadsListDataComposer(group, threads.Skip(startIndex).Take(threads.Count - startIndex).Take(20).ToList(), startIndex));
        }
    }
}