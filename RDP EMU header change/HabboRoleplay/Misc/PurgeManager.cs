using System;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using log4net;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.GameClients;
using System.Linq;

namespace Plus.HabboRoleplay.Misc
{
    public static class PurgeManager
    {
        
        /// <summary>
        /// Set the payday checks
        /// </summary>
        public static void SetTime(int TimeCount)
        {
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                    continue;
                int TimeLeft = RoleplayManager.PurgeTime - TimeCount;
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_purge", "timer," + TimeLeft);
            }
        }
    }
}