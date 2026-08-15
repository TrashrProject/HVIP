using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;

using Plus.Communication.Packets.Outgoing.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class UpdateThreadEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int threadId = packet.PopInt();
            bool pin = packet.PopBoolean();
            bool locked = packet.PopBoolean();

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out group))
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("group_forum_membership_override"))
            {
                if (group.GetForum().ForumModerationSetting == 2 && !group.IsAdmin(session.GetHabbo().Id) && group.CreatorId != session.GetHabbo().Id)
                {
                    session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.admin_create_only"));
                    return;
                }
                else if (group.GetForum().ForumModerationSetting == 3 && group.CreatorId != session.GetHabbo().Id)
                {
                    session.SendNotification(PlusEnvironment.GetGame().GetLanguageLocale().TryGetValue("forum.permissions.creator_only"));
                    return;
                }
            }

            GroupThread Thread = null;
            if (!group.GetForum().TryGetThread(threadId, out Thread))
                return;

            // Mark this thread as update required for the task.
            group.GetForum().UpdateRequired = true;

            if (Thread.Locked != locked)
            {
                Thread.Locked = !Thread.Locked;
                session.SendMessage(new RoomNotificationComposer(Thread.Locked ? "forums.thread.locked" : "forums.thread.unlocked"));
            }
            else if (Thread.Pinned != pin)
            {
                Thread.Pinned = !Thread.Pinned;
                session.SendMessage(new RoomNotificationComposer(Thread.Pinned ? "forums.thread.pinned" : "forums.thread.unpinned"));
            }

            session.SendMessage(new ThreadUpdatedComposer(group, Thread));
        }
    }
}