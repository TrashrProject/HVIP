using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Check if day and night is operating
    /// </summary>
    public class PurgeTimer : SystemRoleplayTimer
    {
        public PurgeTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Executes the day and night process
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (PlusEnvironment.GetGame() == null)
                    return;

                if (PlusEnvironment.GetGame().GetRoomManager() == null)
                    return;

                if (PlusEnvironment.GetGame().GetRoomManager().GetRooms().Count <= 0)
                    return;

                if(!RoleplayManager.PurgeEvent)
                {
                    // Purga detenida
                    RoleplayManager.ReloadData();

                    foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_purge", "timer_off");
                    }

                    base.EndTimer();
                    return;
                }

                if(TimeCount >= RoleplayManager.PurgeTime)
                {
                    // Termina purga
                    RoleplayManager.PurgeEvent = false;
                    RoleplayManager.ReloadData();

                    foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                    {
                        if (client == null || client.GetHabbo() == null || client.GetPlay() == null)
                            continue;

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_purge", "timer_off");
                    }

                    base.EndTimer();
                    return;
                }

                PurgeManager.SetTime(TimeCount);
                RoleplayManager.DeathTime = 1;

                TimeCount++;
            }
            catch(Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}