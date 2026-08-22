using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Database.Interfaces;
using Plus.HabboHotel.Quests;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class RequestBuddyEvent : IPacketEvent
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
                Session.SendNotification("Vous devez posséder un téléphone ParadiseRP pour ajouter des contacts.");
                return;
            }

            if (Session.GetHabbo().GetMessenger().RequestBuddy(Packet.PopString()))
                PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_FRIEND);
        }
    }
}
