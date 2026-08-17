using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class ATMRobTimer : RoleplayTimer
    {
        public ATMRobTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            TimeLeft = base.Client.GetPlay().ATMRobTimeLeft * 1000; // 180 segs => 3 mins.
        }
 
        /// <summary>
        /// Increases the users hunger
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null || base.Client.GetPlay().IsDying || base.Client.GetPlay().IsDead || base.Client.GetPlay().IsJailed || !base.Client.GetPlay().IsRobATM || !base.Client.GetRoomUser().CanWalk || base.Client.GetRoomUser().Frozen || base.Client.GetRoomUser().IsWalking)
                {
                    RoleplayManager.Shout(base.Client, "*Ha dejado de robar el Cajero*", 5);

                    if(base.Client.GetRoomUser().CurrentEffect == EffectsList.Twinkle)
                        base.Client.GetRoomUser().ApplyEffect(0);
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;
                  
                
                //if (base.Client.GetRoomUser().CanWalk)
                  //  base.Client.GetRoomUser().CanWalk = false;
                
                TimeCount++;
                
                TimeLeft -= 1000;

                base.Client.GetPlay().ATMRobTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 5)
                    {
                        base.Client.SendWhisper("Debes esperar " + base.Client.GetPlay().ATMRobTimeLeft + " segundo(s)...", 1);
                        TimeCount = 0;
                    }
                    if (TimeLeft == 120000)
                    {
                        RoleplayManager.Shout(base.Client, "*Golpea repetidamente el metal del Cajero haciendo una pequeña grieta*", 5);
                    }
                    if (TimeLeft == 60000)
                    {
                        RoleplayManager.Shout(base.Client, "*Empuja con todas sus fuerzas en una abertura angosta, ensanchándola aún más*", 5);
                    }
                    return;
                }

                #region Cumple el Timer


                //base.Client.GetRoomUser().CanWalk = true;
                Random rnd = new Random();
                int money = rnd.Next(0, RoleplayManager.MaxATMRobMoney);

                RoleplayManager.Shout(base.Client, "*Roba exitosamente el Cajero Automático [+$ " + money + "]*", 5);

                base.Client.GetHabbo().Credits += money;
                base.Client.GetPlay().MoneyEarned += money;
                base.Client.GetHabbo().UpdateCreditsBalance();
                base.Client.GetPlay().IsRobATM = false;
                base.Client.GetPlay().ATMRobTimeLeft = 0;
                if(base.Client.GetPlay().ATMRobbinItem != null)
                {
                    base.Client.GetPlay().ATMRobbinItem.ExtraData = "1";
                    base.Client.GetPlay().ATMRobbinItem.UpdateState(false, true);
                    base.Client.GetPlay().ATMRobbinItem.RequestUpdate(135 * 5, true);// 5 minutos
                }
                if (base.Client.GetRoomUser().CurrentEffect == EffectsList.Twinkle)
                    base.Client.GetRoomUser().ApplyEffect(0);
                base.EndTimer();
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