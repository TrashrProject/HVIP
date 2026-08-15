using System;
using System.Linq;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Groups.Forums;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ThreadsListDataComposer : ServerPacket
    {
        public ThreadsListDataComposer(Group group, List<GroupThread> threads, int startIndex)
            : base(ServerPacketHeader.ThreadsListDataMessageComposer)
        {
            base.WriteInteger(group.Id);
            base.WriteInteger(startIndex);

            base.WriteInteger(threads.Count);
            foreach (GroupThread thread in threads.ToList())
            {
                base.WriteInteger(thread.Id);
                base.WriteInteger(thread.CreatorId);
                base.WriteString(thread.CreatorUsername);
                base.WriteString(thread.Deleted ? "Thread hidden by " + PlusEnvironment.GetUsernameById(thread.ModeratorId) : thread.Title);
                base.WriteBoolean(thread.Pinned);
                base.WriteBoolean(thread.Locked);
                base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(thread.CreatedAt)).TotalSeconds);
                base.WriteInteger(thread.MessageCount);
                base.WriteInteger(0); // Unread
                base.WriteInteger(0); // ?
                base.WriteInteger(thread.LastReplierId);
                base.WriteString(PlusEnvironment.GetUsernameById(thread.LastReplierId));
                base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(thread.LastReplierDate)).TotalSeconds);
                base.WriteByte(thread.Deleted ? 10 : 0);
                base.WriteInteger(thread.ModeratorId);
                base.WriteString(PlusEnvironment.GetUsernameById(thread.ModeratorId));
                base.WriteInteger(0); //uhmm?
            }
        }
    }
}