using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;


namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class UpdateForumSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int GroupId = packet.PopInt();

            int readabilitySetting = packet.PopInt();
            int postCreationSetting = packet.PopInt();
            int threadCreationSetting = packet.PopInt();
            int forumModerationSetting = packet.PopInt();

            Group group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(GroupId, out group))
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("group_forum_ownership_override"))
            {
                if (session.GetHabbo().Id != group.CreatorId)
                    return;
            }

            if (readabilitySetting < 0 || readabilitySetting > 2)
                readabilitySetting = 0;

            if (postCreationSetting < 0 || postCreationSetting > 3)
                postCreationSetting = 0;

            if (threadCreationSetting < 0 || threadCreationSetting > 3)
                threadCreationSetting = 0;

            if (forumModerationSetting < 2 || forumModerationSetting > 3)
                forumModerationSetting = 2;

            group.GetForum().ReadabilitySetting = readabilitySetting;
            group.GetForum().PostCreationSetting = postCreationSetting;
            group.GetForum().ThreadCreationSetting = threadCreationSetting;
            group.GetForum().ForumModerationSetting = forumModerationSetting;

            group.GetForum().UpdateRequired = true;

            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanModerateSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanPostSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanPostThrdSeen", 1);
            PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModForumCanReadSeen", 1);

            session.SendMessage(new RoomNotificationComposer("forums.forum_settings_updated"));
            session.SendMessage(new ForumDataComposer(group, session.GetHabbo().Id));
        }
    }
}