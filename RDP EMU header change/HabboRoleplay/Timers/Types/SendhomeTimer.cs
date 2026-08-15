using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Sendhome timer
    /// </summary>
    public class SendhomeTimer : RoleplayTimer
    {
        public SendhomeTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            TimeLeft = base.Client.GetPlay().SendHomeTimeLeft * 60000;
        }

        /// <summary>
        /// Pays user after shift
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

                if (base.Client.GetPlay().SendHomeTimeLeft <= 0)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetPlay().SendHomeTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        base.Client.SendWhisper("Te resta(n) " + (TimeLeft / 60000) + " minuto(s) para que puedas volver al trabajo", 1);
                        TimeCount = 0;

                        if (base.Client.GetPlay().SendHomeTimeLeft * 60000 != TimeLeft)
                        {
                            TimeLeft = base.Client.GetPlay().SendHomeTimeLeft * 60000;
                            base.Client.SendWhisper("¡Tu castigo está terminando! Resta(n) " + (TimeLeft / 60000) + " minuto(s)", 1);
                        }
                    }
                    return;
                }

                base.Client.GetPlay().SendHomeTimeLeft = 0;
                RoleplayManager.Shout(base.Client, "*Completa el castigo de su empresa y puede volver al trabajo*", 5);

                base.EndTimer();
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}