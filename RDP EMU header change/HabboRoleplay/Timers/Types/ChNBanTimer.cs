using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to break handcuffs
    /// </summary>
    public class ChNBanTimer : RoleplayTimer
    {
        public ChNBanTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            TimeLeft = base.Client.GetPlay().ChNBannedTimeLeft * 60000;
            //TimeCount = 60 * (8 - base.Client.GetPlay().ChNBannedTimeLeft);
        }
 
        /// <summary>
        /// Removes the cuff
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetPlay().ChNBannedTimeLeft == 0)
                {
                    base.EndTimer();
                    return;
                }


                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetPlay().ChNBannedTimeLeft--;

                if (TimeLeft > 0)
                    return;

                base.Client.GetPlay().ChNBanned = false;
                base.Client.GetPlay().ChNBannedTimeLeft = 0;
                base.EndTimer();
            }
            catch(Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}