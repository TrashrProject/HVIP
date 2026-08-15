using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboHotel.Items;
namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Begins healing the users health
    /// </summary>
    public class HealTimer : RoleplayTimer
    {

        public HealTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params) 
        {
            // Convert to milliseconds
            TimeLeft = RoleplayManager.HealHospitalTime * 1000;
        }

        /// <summary>
        /// Decrease our users timer
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null)
                {
                    base.Client.GetPlay().HeridaName = "";
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetPlay().CurHealth >= base.Client.GetPlay().MaxHealth)
                {
                    base.Client.GetPlay().HeridaName = "";
                    base.Client.GetPlay().CurHealth = base.Client.GetPlay().MaxHealth;
                    base.Client.GetPlay().BeingHealed = false;
                    base.EndTimer();
                    return;
                }

                if (!base.Client.GetPlay().BeingHealed)
                {
                    base.Client.GetPlay().HeridaName = "";
                    base.EndTimer();
                    return;
                }
                Room Room = RoleplayManager.GenerateRoom(Client.GetHabbo().CurrentRoomId);
                string MyCity = Room.City;

                PlayRoom Data;
                int ToHosp = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);

                if (base.Client.GetHabbo().CurrentRoomId != ToHosp)// Hospital de la cd
                {
                    base.Client.GetPlay().HeridaName = "";
                    base.Client.GetPlay().BeingHealed = false;
                    base.Client.SendWhisper("¡Has abandonado el Hospital antes de que tu tratamiento de curación terminara!", 1);
                    base.EndTimer();
                    return;
                }

                TimeLeft -= 1000;

                if (TimeLeft > 0)
                    return;

                int NewHealth;

                if (base.Client.GetHabbo().VIPRank > 0)
                    NewHealth = Random.Next(8, 20);
                else
                    NewHealth = Random.Next(5, 16);

                int CurHealth = base.Client.GetPlay().CurHealth;
                int MaxHealth = base.Client.GetPlay().MaxHealth;

                if (CurHealth + NewHealth < MaxHealth)
                {
                    base.Client.GetPlay().BeingHealed = true;
                    base.Client.GetPlay().CurHealth += NewHealth;
                    TimeLeft = 5 * 1000;
                    base.Client.SendWhisper("Tu saluda ahora es de " + base.Client.GetPlay().CurHealth + "/" + base.Client.GetPlay().MaxHealth, 1);
                    return;
                }
                else
                {
                    base.Client.GetPlay().HeridaName = "";
                    base.Client.GetPlay().BeingHealed = false;
                    base.Client.GetPlay().CurHealth = MaxHealth;
                    base.Client.SendWhisper("Tu saluda está al máximo " + base.Client.GetPlay().CurHealth + "/" + base.Client.GetPlay().MaxHealth, 1);

                    if (base.Client.GetPlay().IsDead)
                    {
                        Client.GetPlay().UpdateTimerDialogue("Dead-Timer", "remove", Client.GetPlay().DeadTimeLeft, OriginalTime);

                        if (base.Client.GetRoomUser().Frozen)
                            base.Client.GetRoomUser().Frozen = false;

                        RoleplayManager.SpawnChairs(base.Client, "val14_wchair");

                        //base.Client.GetPlay().BeingHealed = false;
                        base.Client.GetPlay().IsDead = false;
                        base.Client.GetPlay().DeadTimeLeft = 0;
                        base.Client.GetPlay().InState = false;
                        base.Client.GetPlay().ReplenishStats(true);
                    }

                    base.EndTimer();
                    return;
                }
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.Client.GetPlay().HeridaName = "";
                base.EndTimer();
            }
        }
    }
}