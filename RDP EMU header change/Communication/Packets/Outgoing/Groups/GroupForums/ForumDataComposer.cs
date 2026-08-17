using System;

using Plus.Utilities;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Groups.Forums
{
    class ForumDataComposer : ServerPacket
    {
        public ForumDataComposer(Group group, int userId)
            : base(ServerPacketHeader.ForumDataMessageComposer)
        {
            base.WriteInteger(group.Id);
            base.WriteString(group.Name);
            base.WriteString(group.Description);
            base.WriteString(group.Badge);

            base.WriteInteger(group.GetForum().GetThreads().Count); // Used for pagination
            base.WriteInteger(group.GetForum().Score); // Score?
            base.WriteInteger(group.GetForum().MessageCount); // Total messages
            base.WriteInteger(0); // Unread for this user.
            base.WriteInteger(group.GetForum().MessageCount); // Total again?
            base.WriteInteger(group.GetForum().LastReplierId);
            base.WriteString(PlusEnvironment.GetUsernameById(group.GetForum().LastReplierId));
            base.WriteInteger((int)DateTime.Now.Subtract(UnixTimestamp.FromUnixTimestamp(group.GetForum().LastReplierDate)).TotalSeconds);

            base.WriteInteger(group.GetForum().ReadabilitySetting);
            base.WriteInteger(group.GetForum().PostCreationSetting);
            base.WriteInteger(group.GetForum().ThreadCreationSetting);
            base.WriteInteger(group.GetForum().ForumModerationSetting);
            base.WriteString(group.GetForum().ReadabilitySetting == 1 && (!group.GetMembers.Contains(userId) && !group.GetAdministrators.Contains(userId)) ? "not_member" : group.GetForum().ReadabilitySetting == 2 && !group.GetAdministrators.Contains(userId) ? "not_admin" : "");
            base.WriteString(group.GetForum().PostCreationSetting == 1 && (!group.GetMembers.Contains(userId) && !group.GetAdministrators.Contains(userId)) ? "not_member" : group.GetForum().PostCreationSetting == 2 && !group.GetAdministrators.Contains(userId) ? "not_admin" : group.GetForum().PostCreationSetting == 3 && group.CreatorId != userId ? "not_owner" : "");
            base.WriteString(group.GetForum().ThreadCreationSetting == 1 && (!group.GetMembers.Contains(userId) && !group.GetAdministrators.Contains(userId)) ? "not_member" : group.GetForum().ThreadCreationSetting == 2 && !group.GetAdministrators.Contains(userId) ? "not_admin" : group.GetForum().ThreadCreationSetting == 3 && group.CreatorId != userId ? "not_owner" : "");
            base.WriteString(group.GetForum().ForumModerationSetting == 1 && (!group.GetMembers.Contains(userId) && !group.GetAdministrators.Contains(userId)) ? "not_member" : group.GetForum().ForumModerationSetting == 2 && !group.GetAdministrators.Contains(userId) ? "not_admin" : group.GetForum().ForumModerationSetting == 3 && group.CreatorId != userId ? "not_owner" : "");
            base.WriteString(""); // Not sure.
            base.WriteBoolean(userId == group.CreatorId);
            base.WriteBoolean(true); //Not sure.
        }
    }
}