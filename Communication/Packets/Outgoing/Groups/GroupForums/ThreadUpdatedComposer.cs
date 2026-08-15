using System;
using Plus.Utilities;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ThreadUpdatedComposer : ServerPacket
    {
        public ThreadUpdatedComposer(Group group, GroupThread thread)
            : base(ServerPacketHeader.ThreadUpdatedMessageComposer)
        {
            base.WriteInteger(group.Id);
            base.WriteInteger(thread.Id);
            base.WriteInteger(thread.CreatorId);//Posted by Id
            base.WriteString(thread.CreatorUsername);//Posted by Username
            base.WriteString(thread.Title);//Thread title
            base.WriteBoolean(thread.Pinned);//Is stickied
            base.WriteBoolean(thread.Locked);//Is locked
            base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(thread.CreatedAt)).TotalSeconds);
            base.WriteInteger(thread.MessageCount);//Messages
            base.WriteInteger(0);//Unread
            base.WriteInteger(22073);//?
            base.WriteInteger(thread.LastReplierId);//Last message by Id
            base.WriteString(PlusEnvironment.GetUsernameById(thread.LastReplierId));//Last message by username
            base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(thread.LastReplierDate)).TotalSeconds);//Last update time? value/60/60
            base.WriteByte(thread.Deleted ? 10 : 0);//If 10, then the thread is hidden?
            base.WriteInteger(thread.ModeratorId);//Hidden by Id maybe?
            base.WriteString(PlusEnvironment.GetUsernameById(thread.ModeratorId));//Hidden by username (100%)
            base.WriteInteger(1110);//uhmm?
        }
    }
}