using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Users;

namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class GetForumUserProfileEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int userID = packet.PopInt();

            Habbo targetData = PlusEnvironment.GetHabboById(userID);
            if (targetData == null)
            {
                session.SendNotification("An error occured whilst finding that user's profile.");
                return;
            }

            List<Group> groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(targetData.Id);

            int friendCount = 0;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `messenger_friendships` WHERE (`user_one_id` = @userid OR `user_two_id` = @userid)");
                dbClient.AddParameter("userid", userID);
                friendCount = dbClient.getInteger();
            }

            session.SendMessage(new ProfileInformationComposer(targetData, session, groups, friendCount));
        }
    }
}