using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Database.Interfaces;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class AcceptBuddyEvent : IPacketEvent
    {
        private static bool HasCompatiblePhone(HabboHotel.GameClients.GameClient Session)
        {
            if (Session == null || Session.GetHabbo() == null)
                return false;

            if (Session.GetPlay() != null && Session.GetPlay().Phone > 0)
                return true;

            try
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery(@"SELECT 1
                                      FROM `rp_inventory_items` i
                                      INNER JOIN `rp_item_definitions` d ON d.id = i.item_definition_id
                                      WHERE i.owner_user_id = @userid
                                        AND i.quantity > 0
                                        AND (UPPER(d.effect_type) = 'PHONE' OR UPPER(d.code) = 'PHONE_BASIC')
                                      LIMIT 1");
                    dbClient.AddParameter("userid", Session.GetHabbo().Id);
                    return dbClient.findsResult();
                }
            }
            catch
            {
                return false;
            }
        }

        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            if (!HasCompatiblePhone(Session))
            {
                Session.SendNotification("Vous devez posséder un téléphone ParadiseRP pour accepter des contacts.");
                return;
            }

            int Amount = Packet.PopInt();
            if (Amount > 50)
                Amount = 50;
            else if (Amount < 0)
                return;

            for (int i = 0; i < Amount; i++)
            {
                int RequestId = Packet.PopInt();

                MessengerRequest Request = null;
                if (!Session.GetHabbo().GetMessenger().TryGetRequest(RequestId, out Request))
                    continue;

                if (Request.To != Session.GetHabbo().Id)
                    return;

                if (!Session.GetHabbo().GetMessenger().FriendshipExists(Request.To))
                    Session.GetHabbo().GetMessenger().CreateFriendship(Request.From);

                Session.GetHabbo().GetMessenger().HandleRequest(RequestId);
            }
        }
    }
}
