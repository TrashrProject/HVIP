using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ThreadCreatedComposer : ServerPacket
    {
        public ThreadCreatedComposer(Group group, GroupThread thread)
            : base(ServerPacketHeader.ThreadCreatedMessageComposer)
        {
            base.WriteInteger(group.Id);//GroupId
            base.WriteInteger(thread.Id);//ThreadId
            base.WriteInteger(thread.CreatorId);//Us
            base.WriteString(thread.CreatorUsername);
            base.WriteString(PlusEnvironment.GetHabboById(thread.CreatorId).Look);
            base.WriteBoolean(false);//??
            base.WriteBoolean(false);//??
            base.WriteInteger(0);
            base.WriteInteger(1);
            base.WriteInteger(1);
            base.WriteInteger(3);
            base.WriteInteger(thread.CreatorId);//Us
            base.WriteString("");
            base.WriteInteger(0);
            base.WriteByte(thread.Deleted ? 10 : 0);
            base.WriteInteger(thread.ModeratorId);//Hidden by Id maybe?
            base.WriteString(PlusEnvironment.GetUsernameById(thread.ModeratorId));//Hidden by username
            base.WriteInteger(PlusEnvironment.GetHabboById(thread.CreatorId).GetStats().ForumPosts);//messages/posts?
        }
    }
}