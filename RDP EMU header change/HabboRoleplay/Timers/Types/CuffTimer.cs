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
    public class CuffTimer : RoleplayTimer
    {
        public CuffTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // 8 minutes converted to miliseconds
            TimeLeft = base.Client.GetPlay().CuffedTimeLeft * 60000;
            TimeCount = 60 * (8 - base.Client.GetPlay().CuffedTimeLeft);
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

                if (!base.Client.GetPlay().Cuffed || base.Client.GetPlay().CuffedTimeLeft == 0)
                {
                    if (base.Client.GetRoomUser() != null && !base.Client.GetRoomUser().CanWalk)
                        base.Client.GetRoomUser().CanWalk = true;
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetPlay().IsEscorted)
                    return;

                if (base.Client.GetRoomUser() != null)
                {
                    if (base.Client.GetRoomUser().CanWalk)
                        base.Client.GetRoomUser().CanWalk = false;
                }
                

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60 || TimeCount == 60 * 2 || TimeCount == 60 * 3 || TimeCount == 60 * 4 || TimeCount == 60 * 5 || TimeCount == 60 * 6 || TimeCount == 60 * 7)
                {
                    if (TimeCount == 60)
                        base.Client.SendWhisper("Comienzas a forzar las esposas para intentar liberarte.", 1);
                    else if (TimeCount == 60 * 2)
                        base.Client.SendWhisper("Tus muñecas comienzan a doler mientras continúas tratando de romper las esposas.", 1);
                    else if (TimeCount == 60 * 3)
                        base.Client.SendWhisper("Tus muñecas empiezan a deformarse a la medida de las esposas.", 1);
                    else if (TimeCount == 60 * 4)
                        base.Client.SendWhisper("El pulgar de tu mano derecha pudo salir de las esposas.", 1);
                    else if (TimeCount == 60 * 5)
                        base.Client.SendWhisper("Tu pulgar se ha dislocado y suena un fuerte crujido.", 1);
                    else if (TimeCount == 60 * 6)
                        base.Client.SendWhisper("Sigues luchando tratando de sacar una mano por completo de las esposas.", 1);
                    else if (TimeCount == 60 * 7)
                        base.Client.SendWhisper("Intentas hacer lo mismo con tu mano izquiera para liberarte por completo.", 1);

                    base.Client.GetPlay().CuffedTimeLeft--;
                }

                if (TimeLeft > 0)
                    return;

                base.Client.GetPlay().Cuffed = false;
                base.Client.GetPlay().CuffedTimeLeft = 0;
                RoleplayManager.Shout(base.Client, "*Rompe las esposas de sus muñecas y se libera*", 5);
                base.Client.GetRoomUser().Frozen = false;
                base.Client.GetRoomUser().CanWalk = true;
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