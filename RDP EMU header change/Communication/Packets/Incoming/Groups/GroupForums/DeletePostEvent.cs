using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class DeletePostEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            int threadId = packet.PopInt();
            int postId = packet.PopInt();
            int status = packet.PopInt();

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

            GroupThread thread = null;
            if (!group.GetForum().TryGetThread(threadId, out thread))
                return;

            GroupPost post = null;
            if (!thread.TryGetPost(postId, out post))
            {
                //No post was found, are we trying to delete the thread?
                if (postId == threadId)
                {
                    thread.Deleted = !thread.Deleted;
                    thread.Pinned = false;
                    thread.ModeratorId = (thread.Deleted ? session.GetHabbo().Id : 0);

                    // Mark this thread as update required for the task.
                    group.GetForum().UpdateRequired = true;

                    session.SendMessage(new ThreadDataComposer(group, thread, thread.GetPosts().ToList(), 0));
                    session.SendMessage(new RoomNotificationComposer(thread.Deleted ? "forums.message.hidden" : "forums.message.restored"));
                }
                return;
            }

            post.Deleted = !post.Deleted;
            post.ModeratorId = (post.Deleted ? session.GetHabbo().Id : 0);

            // Mark this thread as update required for the task.
            group.GetForum().UpdateRequired = true;

            session.SendMessage(new PostUpdatedComposer(group, post));
            session.SendMessage(new RoomNotificationComposer(post.Deleted ? "forums.message.hidden" : "forums.message.restored"));
        }
    }
}