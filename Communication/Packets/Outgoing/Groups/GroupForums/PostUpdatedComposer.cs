using System;

using Plus.Utilities;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class PostUpdatedComposer : ServerPacket
    {
        public PostUpdatedComposer(Group group, GroupPost post)
            : base(ServerPacketHeader.PostUpdatedMessageComposer)
        {
            base.WriteInteger(group.Id);
            base.WriteInteger(post.ThreadId);
            base.WriteInteger(post.Id);//Message Id
            base.WriteInteger(post.OrderId);//Message Index
            base.WriteInteger(post.CreatorId);//User Id
            base.WriteString(post.CreatorUsername);//Username
            base.WriteString(PlusEnvironment.GetHabboById(post.CreatorId).Look);//Figure
            base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(post.CreatedAt)).TotalSeconds);
            base.WriteString(post.Content);//Content
            base.WriteByte(post.Deleted ? 10 : 0);//If 10, then the thread is hidden?
            base.WriteInteger(post.ModeratorId);//Hidden by Id maybe?
            base.WriteString(PlusEnvironment.GetUsernameById(post.ModeratorId));//Hidden by username
            base.WriteInteger(0);//uhmm?     
            base.WriteInteger(PlusEnvironment.GetHabboById(post.CreatorId).GetStats().ForumPosts);//messages/posts?
        }
    }
}