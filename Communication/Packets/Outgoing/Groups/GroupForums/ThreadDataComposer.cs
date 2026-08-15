using System;
using System.Linq;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ThreadDataComposer : ServerPacket
    {
        public ThreadDataComposer(Group group, GroupThread thread, List<GroupPost> posts, int startIndex)
            : base(ServerPacketHeader.ThreadDataMessageComposer)
        {
            base.WriteInteger(group.Id);//Group Id
            base.WriteInteger(thread.Id);//Thread Id
            base.WriteInteger(startIndex);//Start Index

            base.WriteInteger(posts.Count);//Amount
            foreach (GroupPost post in posts)
            {
                base.WriteInteger(post.Id);//Message Id
                base.WriteInteger(post.OrderId);//Message Index
                base.WriteInteger(post.CreatorId);//User Id
                base.WriteString(post.CreatorUsername);//Username
                base.WriteString(PlusEnvironment.GetHabboById(post.CreatorId).Look);//Figure
                base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(post.CreatedAt)).TotalSeconds);
                base.WriteString(post.Deleted ? "Content hidden by " + PlusEnvironment.GetUsernameById(post.ModeratorId) : post.Content);//Content
                base.WriteByte(post.Deleted ? 10 : 0);//If 10, then the thread is hidden?
                base.WriteInteger(post.ModeratorId);//Hidden by Id maybe?
                base.WriteString(PlusEnvironment.GetUsernameById(post.ModeratorId));//Hidden by username
                base.WriteInteger(0);//uhmm?     
                base.WriteInteger(PlusEnvironment.GetHabboById(post.CreatorId).GetStats().ForumPosts);//messages/posts?
            }
        }
    }
}