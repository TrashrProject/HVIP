using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.RolePlay.PlayRoom;
namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Prison timer
    /// </summary>
    public class SancTimer : RoleplayTimer
    {
        public SancTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            OriginalTime = Client.GetPlay().SancTimeLeft;
            TimeLeft = Client.GetPlay().SancTimeLeft * 60000;
            
            Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "add", Client.GetPlay().SancTimeLeft, OriginalTime);            

        }

        /// <summary>
        /// Prison timer
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

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetPlay().PassiveMode)
                {
                    #region Force Desactivate PSV Mode
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_psv", "forceoff");
                    #endregion
                }

                if (base.Client.GetPlay().OwnedWeapons != null)
                {
                    base.Client.GetPlay().OwnedWeapons = null;
                    RoleplayManager.DropAllMyWeapon(base.Client);
                    base.Client.GetPlay().OwnedWeapons = base.Client.GetPlay().LoadAndReturnWeapons();
                }

                #region Liberado #1
                if (!base.Client.GetPlay().IsSanc)
                {
                    Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "remove", Client.GetPlay().JailedTimeLeft, OriginalTime);

                    if (base.Client.GetHabbo().CurrentRoomId != 1)//centro de la cd
                        RoleplayManager.SendUserOld(base.Client, 1, "");

                    RoleplayManager.Shout(base.Client, "*Cumple su castigo y es liberad@*", 5);

                    base.Client.GetPlay().IsSanc = false;
                    base.Client.GetPlay().SancTimeLeft = 0;
                    base.Client.GetPlay().InState = false;

                    base.EndTimer();
                    return;
                }
                #endregion

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeCount == 60)
                    base.Client.GetPlay().SancTimeLeft--;

                if(!base.Client.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    base.Client.GetHabbo().TimeMuted = base.Client.GetPlay().SancTimeLeft * 60;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "decrement", Client.GetPlay().JailedTimeLeft, OriginalTime);            
                        base.Client.SendWhisper("Te resta(n) " + base.Client.GetPlay().JailedTimeLeft + " minuto(s) para ser liberad@ de la sanción.", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                #region Liberado #2
                if (base.Client.GetRoomUser() != null)
                {
                    if (base.Client.GetHabbo().CurrentRoomId != 1)//centro de la cd
                        RoleplayManager.SendUserOld(base.Client, 1, "");

                }

                RoleplayManager.Shout(base.Client, "*Cumple su castigo y es liberad@*", 5);

                base.Client.GetPlay().InState = false;
                base.Client.GetPlay().IsSanc = false;
                base.Client.GetPlay().SancTimeLeft = 0;
                base.Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "remove", Client.GetPlay().JailedTimeLeft, OriginalTime);
                
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