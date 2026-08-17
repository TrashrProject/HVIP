using System;
using System.Linq;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Quests;
using System.Collections.Generic;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using Plus.Utilities;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboRoleplay.Events.Methods
{
    /// <summary>
    /// Triggered when the user's health changes
    /// </summary>
    public class OnHealthChange : IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;
            if (Client == null || Client.GetPlay() == null || Client.GetHabbo() == null || Client.GetPlay().IsDead)
                return;

            if (Client.GetPlay().CurHealth <= 0 && !Client.GetPlay().IsJailed && !Client.GetPlay().IsDying && !Client.GetPlay().IsDead)
            {
               // Client.GetPlay().BeingHealed = false;
                Client.GetPlay().CloseInteractingUserDialogues();
                Client.GetPlay().ClearWebSocketDialogue(true);

                NormalDeath(Client);
            }
            else
                Client.GetPlay().UpdateInteractingUserDialogues();

            Client.GetPlay().RefreshStatDialogue();
                
            if (/*Client.GetPlay().BeingHealed || */Client.GetPlay().CurHealth <= 0 || Client.GetPlay().CurHealth >= Client.GetPlay().MaxHealth)
                return;

            if (Client.GetPlay().Hunger >= 100 && Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("hunger"))
            {
                int TimeCount = Client.GetPlay().TimerManager.ActiveTimers["hunger"].TimeCount;

                if (TimeCount == 0)
                    Client.SendWhisper("Tu salud ha bajado debido a que estás hambrient@. [" + Client.GetPlay().CurHealth + "/" + Client.GetPlay().MaxHealth + "]! ¡Será mejor que comas algo antes de que pierdas toda tu salud!", 1);
                //else
                  //  RoleplayManager.Shout(Client, "*[" + Client.GetPlay().CurHealth + "/" + Client.GetPlay().MaxHealth + "]*", 3);
            }
            //else
              //  RoleplayManager.Shout(Client, "*[" + Client.GetPlay().CurHealth + "/" + Client.GetPlay().MaxHealth + "]*", 3);
        }

        /// <summary>
        /// Kills the user normally, sends them to the hospital
        /// </summary>
        /// <param name="Client"></param>
        private void NormalDeath(GameClient Client)
        {
            if (!Client.GetPlay().IsDead)
            {
                if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("dying") && !Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("death"))
                {
                    RoleplayManager.Shout(Client, "*Cae colapsandose en el suelo y pierde la consciencia*", 32);
                    Client.GetHabbo().HomeRoom = Client.GetRoomUser().RoomId;
                    /*
                    Client.SendWhisper("¡Has muerto! Usa ':servicio medico' para llamar a una ambulancia ó ':aceptarmuerte' para reaparecer en el hospital.", 1);

                    if (Client.GetRoomUser() != null)
                        Client.GetRoomUser().ApplyEffect(0);

                    Client.GetRoomUser().Frozen = true;
                    Client.GetRoomUser().IsWalking = false;
                    */
                    #region Lays User Down (off)
                    /*
                    if (Client.GetRoomUser() != null)
                    {
                        var User = Client.GetRoomUser();

                        if (User.isLying)
                        {
                            User.RemoveStatus("lay");
                            User.isLying = false;
                        }

                        if (User.isSitting)
                        {
                            User.RemoveStatus("sit");
                            User.isSitting = false;
                        }

                        if ((User.RotBody % 2) == 0)
                        {
                            if (User == null)
                                return;

                            try
                            {
                                User.Statusses.Add("lay", "1.0 null");
                                User.Z -= 0.35;
                                User.isLying = true;
                                User.UpdateNeeded = true;
                            }
                            catch { }
                        }
                        else
                        {
                            User.RotBody--;
                            User.Statusses.Add("lay", "1.0 null");
                            User.Z -= 0.35;
                            User.isLying = true;
                            User.UpdateNeeded = true;
                        }
                    }*/
                    #endregion
                    
                    /*
                    if (Client.GetPlay().IsWorking)
                    {
                        WorkManager.RemoveWorkerFromList(Client);
                        Client.GetPlay().IsWorking = false;
                    }
                    */

                    Client.GetPlay().IsDying = true;
                    Client.GetPlay().DyingTimeLeft = RoleplayManager.DyingTime;
                    Client.GetPlay().TimerManager.CreateTimer("dying", 1000, true);
                }
            }
        }
    }
}