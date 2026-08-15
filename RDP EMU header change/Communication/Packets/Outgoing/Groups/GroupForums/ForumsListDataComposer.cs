using System;
using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.Utilities;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ForumsListDataComposer : ServerPacket
    {
        public ForumsListDataComposer(List<Group> groups, int type, int startIndex, int groupCount)
            : base(ServerPacketHeader.ForumsListDataMessageComposer)
        {
            base.WriteInteger(type);
            base.WriteInteger(groupCount);
            base.WriteInteger(startIndex);

            base.WriteInteger(groups.Count);//Amount
            foreach (Group group in groups.ToList())
            {
                base.WriteInteger(group.Id);
                base.WriteString(group.Name);
                base.WriteString(group.Description);
                base.WriteString(group.Badge);
                base.WriteInteger(169); //This seems to be important, but idk what its for
                base.WriteInteger(group.GetForum().Score);
                base.WriteInteger(group.GetForum().MessageCount);
                base.WriteInteger(0);//Unread for this user.
                base.WriteInteger(group.GetForum().MessageCount);
                base.WriteInteger(group.GetForum().LastReplierId);
                base.WriteString(PlusEnvironment.GetUsernameById(group.GetForum().LastReplierId));
                base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(group.GetForum().LastReplierDate)).TotalSeconds);
            }
        }
    }
}