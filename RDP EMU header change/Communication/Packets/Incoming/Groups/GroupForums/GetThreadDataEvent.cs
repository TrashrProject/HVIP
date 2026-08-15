using System;
using System.Linq;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class GetThreadDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int threadId = packet.PopInt();
            int startIndex = packet.PopInt();
            int unknown = packet.PopInt();

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
                return;

            if (!group.ForumEnabled || group.GetForum() == null)
            {
                session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.group.not_found"));
                return;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("group_forum_membership_override"))
            {
                if (group.GetForum().ReadabilitySetting > 0 && !group.IsMember(session.GetHabbo().Id))
                {
                    session.SendNotification("Oops, this forum only allows members and above to view its content.");
                    return;
                }
            }

            GroupThread thread = null;
            if (!group.GetForum().TryGetThread(threadId, out thread))
                return;

            if (thread.Deleted)
            {
                if (group.GetForum().ForumModerationSetting == 2 && !group.IsAdmin(session.GetHabbo().Id) && group.CreatorId != session.GetHabbo().Id)
                {
                    session.SendMessage(new RoomNotificationComposer("forums.error.access_denied"));
                    return;
                }
                else if (group.GetForum().ForumModerationSetting == 3 && group.CreatorId != session.GetHabbo().Id)
                {
                    session.SendMessage(new RoomNotificationComposer("forums.error.access_denied"));
                    return;
                }
            }

            thread.Views++;

            session.SendMessage(new ThreadDataComposer(group, thread, thread.GetPosts().Skip(startIndex).Take(thread.GetPosts().Count - startIndex).Take(20).ToList(), startIndex));
        }
    }
}