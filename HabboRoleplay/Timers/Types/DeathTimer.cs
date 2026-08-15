using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Waits specified time then releases user from hospital
    /// </summary>
    public class DeathTimer : RoleplayTimer
    {

        public DeathTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params) 
        {
            // Convert to milliseconds
            OriginalTime = RoleplayManager.DeathTime;
            TimeLeft = Client.GetPlay().DeadTimeLeft * 60000;
            Client.GetPlay().UpdateTimerDialogue("Dead-Timer", "add", Client.GetPlay().DeadTimeLeft, OriginalTime);            
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
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                // Asignamos una herida si no la tiene.
                if(base.Client.GetPlay().HeridaName == null || base.Client.GetPlay().HeridaName == "")
                {
                    #region Heridas
                    Random rnds = new Random();
                    int heri = rnds.Next(1, 11);
                    if (base.Client.GetPlay().HeridaPaci <= 0)
                    {
                        if (heri == 1)
                        {
                            base.Client.GetPlay().HeridaName = "Herida de bala";
                            base.Client.GetPlay().HeridaPaci = 1;
                        }
                        else if (heri == 2)
                        {
                            base.Client.GetPlay().HeridaName = "Múltiples heridas de bala";
                            base.Client.GetPlay().HeridaPaci = 2;
                        }
                        else if (heri == 3)
                        {
                            base.Client.GetPlay().HeridaName = "Fracturas Graves";
                            base.Client.GetPlay().HeridaPaci = 3;
                        }
                        else if (heri == 4)
                        {
                            base.Client.GetPlay().HeridaName = "Fracturas leves";
                            base.Client.GetPlay().HeridaPaci = 4;
                        }
                        else if (heri == 5)
                        {
                            base.Client.GetPlay().HeridaName = "Herida abierta de sangre";
                            base.Client.GetPlay().HeridaPaci = 5;
                        }
                        else if (heri == 6)
                        {
                            base.Client.GetPlay().HeridaName = "Hematomas";
                            base.Client.GetPlay().HeridaPaci = 6;
                        }
                        else if (heri == 7)
                        {
                            base.Client.GetPlay().HeridaName = "Hematomas y huesos fracturados";
                            base.Client.GetPlay().HeridaPaci = 7;
                        }
                        else if (heri == 8)
                        {
                            base.Client.GetPlay().HeridaName = "Hemorragia cerebral";
                            base.Client.GetPlay().HeridaPaci = 8;
                        }
                        else if (heri == 9)
                        {
                            base.Client.GetPlay().HeridaName = "Quemaduras";
                            base.Client.GetPlay().HeridaPaci = 9;
                        }
                        else if (heri == 10)
                        {
                            base.Client.GetPlay().HeridaName = "Daños severos";
                            base.Client.GetPlay().HeridaPaci = 10;
                        }
                        else
                        {
                            base.Client.GetPlay().HeridaName = "Herida de bala";
                            base.Client.GetPlay().HeridaPaci = 1;
                        }
                    }
                    #endregion
                }

                // Confiscamos armas solo si no murió en un interior.
                if (base.Client.GetPlay().OwnedWeapons != null && base.Client.GetPlay().DeathInOut && !RoleplayManager.PurgeEvent)
                {
                    base.Client.GetPlay().OwnedWeapons = null;
                    RoleplayManager.DropAllMyWeapon(base.Client);
                    base.Client.GetPlay().OwnedWeapons = base.Client.GetPlay().LoadAndReturnWeapons();
                    base.Client.GetPlay().DeathInOut = false;
                }

                #region Revivido #1 (endtimer)
                if (!base.Client.GetPlay().IsDead)
                {
                    Client.GetPlay().UpdateTimerDialogue("Dead-Timer", "remove", Client.GetPlay().DeadTimeLeft, OriginalTime);

                    if (base.Client.GetRoomUser().Frozen)
                      base.Client.GetRoomUser().Frozen = false;

                    //if (base.Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2")))
                    //RoleplayManager.SpawnChairs(base.Client, "val14_wchair");
                    #region IN CHAIR By Jeihden
                    Room Room2 = base.Client.GetHabbo().CurrentRoom;

                    Item BTile2 = Room2.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "val14_wchair" && x.Coordinate == base.Client.GetRoomUser().Coordinate);
                    if (BTile2 == null)
                        RoleplayManager.SpawnChairs(base.Client, "val14_wchair");
                    #endregion

                    //base.Client.GetPlay().BeingHealed = false;
                    base.Client.GetPlay().HeridaName = "";
                    base.Client.GetPlay().HeridaPaci = 0;
                    base.Client.GetPlay().IsDead = false;
                    base.Client.GetPlay().DeadTimeLeft = 0;
                    base.Client.GetPlay().InState = false;
                    base.Client.GetHabbo().Poof(true);
                    base.Client.GetPlay().ReplenishStats(true);
                    base.EndTimer();
                    return;
                }
                #endregion

                if (!base.Client.GetRoomUser().Frozen)
                    base.Client.GetRoomUser().Frozen = true;

                //if (base.Client.GetPlay().BeingHealed)
                  //  return;

                TimeCount++;
                TimeLeft -= 1000;

                #region IN STATE CONDITIONS By Jeihden
                // Verificar si está en una camilla.
                if (!base.Client.GetPlay().InState)
                {
                    Room Room3 = base.Client.GetHabbo().CurrentRoom;

                    Item BTile3 = Room3.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "hosptl_bed" && x.Coordinate == base.Client.GetRoomUser().Coordinate);
                    if(BTile3 == null)
                        RoleplayManager.SpawnBeds(Client, "hosptl_bed");
                    else
                        base.Client.GetPlay().InState = true;
                }
                #endregion

                // Animación de curación.
                if (TimeCount % 9 == 0 && base.Client.GetPlay().CurHealth+3 < base.Client.GetPlay().MaxHealth)
                {
                    base.Client.GetPlay().CurHealth += 3;
                    base.Client.GetPlay().RefreshStatDialogue();

                }

                // Cada minuto.
                if (TimeCount == 60)
                    base.Client.GetPlay().DeadTimeLeft--;

                // Mensaje al Usuario cada minuto.
                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        Client.GetPlay().UpdateTimerDialogue("Dead-Timer", "decrement", Client.GetPlay().DeadTimeLeft, OriginalTime);
                        base.Client.SendWhisper("Resta(n) " + base.Client.GetPlay().DeadTimeLeft + " minuto(s) para que seas dad@ de alta del Hospital.", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                #region Revivido #2 (endtimer)
                if (base.Client.GetRoomUser().Frozen)
                    base.Client.GetRoomUser().Frozen = false;

                //if (base.Client.GetRoomUser().RoomId == Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2")))
                //RoleplayManager.SpawnChairs(base.Client, "val14_wchair");
                #region IN CHAIR By Jeihden
                Room Room = base.Client.GetHabbo().CurrentRoom;

                Item BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "val14_wchair" && x.Coordinate == base.Client.GetRoomUser().Coordinate);
                if (BTile == null)
                    RoleplayManager.SpawnChairs(base.Client, "val14_wchair");
                #endregion

                Client.GetPlay().UpdateTimerDialogue("Dead-Timer", "remove", Client.GetPlay().DeadTimeLeft, OriginalTime);

                RoleplayManager.Shout(base.Client, "*Recupera la consciencia y es dad@ de alta del Hospital*", 5);

                //base.Client.GetPlay().BeingHealed = false;
                base.Client.GetPlay().HeridaName = "";
                base.Client.GetPlay().HeridaPaci = 0;
                base.Client.GetPlay().IsDead = false;
                base.Client.GetPlay().DeadTimeLeft = 0;
                base.Client.GetPlay().InState = false;
                base.Client.GetHabbo().Poof(true);
                base.Client.GetPlay().ReplenishStats(true);
                base.Client.GetPlay().RefreshStatDialogue();
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