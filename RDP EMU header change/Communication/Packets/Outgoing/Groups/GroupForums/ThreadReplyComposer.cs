using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ThreadReplyComposer : ServerPacket
    {
        public ThreadReplyComposer(Group group, GroupThread thread, GroupPost post)
            : base(ServerPacketHeader.ThreadReplyMessageComposer)
        {
            base.WriteInteger(group.Id);//GroupId
            base.WriteInteger(thread.Id);//ThreadId
            base.WriteInteger(post.Id);//MessageId
            base.WriteInteger(post.OrderId);//Message Index
            base.WriteInteger(post.CreatorId);//Us
            base.WriteString(post.CreatorUsername);
            base.WriteString(PlusEnvironment.GetHabboById(post.CreatorId).Look);
            base.WriteInteger(thread.MessageCount);
            base.WriteString(post.Content);
            base.WriteByte(post.Deleted ? 10 : 0);//If 10, then the thread is hidden?
            base.WriteInteger(post.ModeratorId);//Hidden by Id maybe?
            base.WriteString(PlusEnvironment.GetUsernameById(post.ModeratorId));//Hidden by username
            base.WriteInteger(0);//uhmm?     
            base.WriteInteger(PlusEnvironment.GetHabboById(post.CreatorId).GetStats().ForumPosts);//messages/posts?
        }
    }
}