using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class HungerTimer : RoleplayTimer
    {
        public HungerTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {

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

                if (base.Client.GetRoomUser() == null || base.Client.GetRoomUser().GetRoom().NoHungerEnabled)
                    return;

                if (base.Client.GetRoomUser().IsAsleep || base.Client.GetPlay().IsDead || base.Client.GetPlay().IsDying || base.Client.GetPlay().Cuffed || base.Client.GetPlay().IsJailed || base.Client.GetPlay().IsSanc)
                    return;

                if (base.Client.GetHabbo().EscortID > 0 || base.Client.GetHabbo().Escorting > 0 || base.Client.GetHabbo().TaxiChofer > 0 || base.Client.GetHabbo().TaxiPassenger > 0)
                    return;
                
                TimeCount++;
                
                if (TimeCount < ((base.Client.GetHabbo().VIPRank == 2) ? (RoleplayManager.HungerTime * 2) : RoleplayManager.HungerTime) )//Cada 300 segundos sube el hambre.
                    return;

                int AmountOfHunger = Random.Next(1, 5);
                base.Client.GetPlay().Hunger += AmountOfHunger;
                base.Client.GetPlay().RefreshStatDialogue();
                TimeCount = 0;

                if (base.Client.GetPlay().Hunger < 100)
                    return;

                base.Client.GetPlay().Hunger = 100;
                // Si está encarcelado, que no muera por hambre.
                if (base.Client.GetPlay().CurHealth - 5 <= 0 && base.Client.GetPlay().IsJailed)
                  return;

                if (base.Client.GetPlay().CurHealth - 5 <= 0)
                {
                    base.Client.GetPlay().CurHealth = 0;// Vida en 0 por hambre.
                    Client.GetPlay().TimerManager.CreateTimer("dying", 1000, true);
                }
                else
                {
                    //base.Client.SendWhisper("You have lost 5 HP as you have not ate in days! Replenish your hunger to avoid losing more health!", 1);
                    //base.Client.SendMessage(new RoomNotificationComposer("hunger_high_warning", "message", "¡Estás muy hambrient@! Come algo antes de que pierdas toda tu salud hasta parar en el Hospital."));
                    base.Client.GetPlay().CurHealth -= 5;
                }

                base.Client.GetPlay().RefreshStatDialogue();
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}