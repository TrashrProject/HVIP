using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.VehicleOwned;
using System.Collections.Generic;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class VehicleJobTimer : RoleplayTimer
    {
        public VehicleJobTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            TimeLeft = base.Client.GetPlay().VehicleTimer * 1000; // 5 mins, polis 10 mins
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

                if (base.Client.GetPlay().IsDying || base.Client.GetPlay().IsDead || base.Client.GetPlay().IsJailed)
                {
                    RoleplayManager.CheckCorpCarp(base.Client);
                    base.Client.GetPlay().CamCargId = 0;
                    base.EndTimer();
                    base.Client.SendWhisper("¡Tu Vehículo de trabajo ha sido regresado a su sitio por haberlo abandonado mucho tiempo!", 1);
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetPlay().DrivingCar && base.Client.GetPlay().CarJobLastItemId > 0)
                    return;
                                
                TimeCount++;
                
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                {
                    if(TimeLeft % 60000 == 0)
                    {
                        base.Client.SendWhisper("Te queda(n) " + (TimeLeft / 60000) + " minuto(s) para volver a tu vehículo de trabajo. ¡Si no vuelves será decomisado!", 1);
                    }
                    return;
                }

                #region Cumple el Timer
                RoleplayManager.CheckCorpCarp(base.Client);
                base.Client.GetPlay().CamCargId = 0;
                base.EndTimer();
                base.Client.SendWhisper("¡Tu Vehículo de trabajo ha sido regresado a su sitio por haberlo abandonado mucho tiempo!", 1);
                #endregion                
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}