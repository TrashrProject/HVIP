using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Noob timer
    /// </summary>
    public class NoobTimer : RoleplayTimer
    {
        public NoobTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 15 minutes convert to milliseconds (Default in play_stats)
            TimeLeft = Client.GetPlay().NoobTimeLeft * 60000;
        }
 
        /// <summary>
        /// Increases the users hunger
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

                if (!base.Client.GetPlay().IsNoob)
                    return;

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                /*
                if (base.Client.GetHabbo().CurrentRoom != null)
                {
                    if (base.Client.GetHabbo().CurrentRoom.TutorialEnabled)
                        return;
                }
                */

                if (base.Client.GetHabbo().CurrentRoom == null)
                    return;

                if (base.Client.GetHabbo().CurrentRoomId <= 0)
                    return;

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetPlay().NoobTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        //base.Client.SendWhisper("You have " + (TimeLeft / 60000) + " minute(s) remaining till you lose your God Protection!", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                base.Client.SendNotification("¡Tu Protección de inmunidad de Usuario nuevo se ha terminado! Ahora podrás recibir y hacer daño a otros jugadores. ¡Buena suerte y disfruta del Rol en su máxima expresión! ");
                base.Client.GetPlay().IsNoob = false;
                base.Client.GetPlay().NoobTimeLeft = 0;
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