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
    public class JailTimer : RoleplayTimer
    {
        public JailTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            OriginalTime = Client.GetPlay().WantedLevel * RoleplayManager.StarsJailTime;
            TimeLeft = Client.GetPlay().JailedTimeLeft * 60000;
            
            Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "add", Client.GetPlay().JailedTimeLeft, OriginalTime);            

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

                if(base.Client.GetPlay().OwnedWeapons != null)
                {
                    base.Client.GetPlay().OwnedWeapons = null;
                    RoleplayManager.DropAllMyWeapon(base.Client);
                    base.Client.GetPlay().OwnedWeapons = base.Client.GetPlay().LoadAndReturnWeapons();
                }

                if (base.Client.GetPlay().PassiveMode)
                {
                    #region Force Desactivate PSV Mode
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_psv", "forceoff");
                    #endregion
                }

                #region Liberado #1
                if (!base.Client.GetPlay().IsJailed)
                {
                    Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "remove", Client.GetPlay().JailedTimeLeft, OriginalTime);

                    Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                    string MyCity = Room.City;

                    PlayRoom Data;
                    int ToJail = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetJail(MyCity, out Data);
                    int ToJailback = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetJailBack(MyCity, out Data);

                    if (base.Client.GetHabbo().CurrentRoomId == ToJail || base.Client.GetHabbo().CurrentRoomId == ToJailback)//prision de la cd
                        RoleplayManager.SendUser(base.Client, ToJail, "");
                    //RoleplayManager.SpawnChairs(base.Client, "comodin_carr2");

                    RoleplayManager.Shout(base.Client, "*Cumple su condena en prisión y es puest@ en libertad*", 5);
                    /*
                    if (base.Client.GetPlay().JailedTimeLeft != -5)
                    {
                        base.Client.GetPlay().OnProbation = true;
                        base.Client.GetPlay().ProbationTimeLeft = 5;
                        base.Client.GetPlay().TimerManager.CreateTimer("probation", 1000, false);
                        base.Client.SendWhisper("You have been placed on probation for " + base.Client.GetPlay().ProbationTimeLeft + " minutes!", 1);
                        base.Client.SendWhisper("If you commit any crimes during probation you will be wanted an extra star level!", 1);
                    }
                    */

                    base.Client.GetPlay().WantedFor = "";
                    //base.Client.GetPlay().Trialled = false;
                    //base.Client.GetPlay().Jailbroken = false;
                    base.Client.GetPlay().IsJailed = false;
                    base.Client.GetPlay().JailedTimeLeft = 0;
                    base.Client.GetPlay().InState = false;

                    // WS Wanted Stars
                    if (base.Client.GetPlay().WebSocketConnection != null)
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(base.Client, "compose_wanted_stars|" + base.Client.GetPlay().WantedLevel);

                    base.Client.GetHabbo().Poof(true);
                    RoleplayManager.GetLookAndMotto(base.Client);
                    base.EndTimer();
                    return;
                }
                #endregion

                //if (base.Client.GetPlay().Jailbroken)
                //  return;

                TimeCount++;
                TimeLeft -= 1000;

                #region IN STATE CONDITIONS By Jeihden
                // Verificar si está en una celda.
                if (!base.Client.GetPlay().InState)
                {
                    //Room Room3 = base.Client.GetHabbo().CurrentRoom;

                    //Item BTile3 = Room3.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "bed_silo_one" && x.Coordinate == base.Client.GetRoomUser().Coordinate);
                    //if (BTile3 == null)
                    RoleplayManager.GetLookAndMotto(Client);
                    RoleplayManager.SpawnBeds(Client, "bed_silo_one");
                    //else
                        base.Client.GetPlay().InState = true;
                }
                #endregion

                if (TimeCount == 60)
                    base.Client.GetPlay().JailedTimeLeft--;

                if (base.Client.GetPlay().Cuffed)
                    base.Client.GetPlay().Cuffed = false;

                if (RoleplayManager.WantedList.ContainsKey(base.Client.GetHabbo().Id))
                {
                    Wanted Junk;
                    RoleplayManager.WantedList.TryRemove(base.Client.GetHabbo().Id, out Junk);
                }

                if (base.Client.GetPlay().IsWanted || base.Client.GetPlay().WantedLevel != 0 || base.Client.GetPlay().WantedTimeLeft != 0)
                {
                    base.Client.GetPlay().IsWanted = false;
                    base.Client.GetPlay().WantedLevel = 0;
                    base.Client.GetPlay().WantedTimeLeft = 0;
                    // WS Wanted Stars
                    if (base.Client.GetPlay().WebSocketConnection != null)
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(base.Client, "compose_wanted_stars|" + base.Client.GetPlay().WantedLevel);
                    UpdateJailLook();
                }

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "decrement", Client.GetPlay().JailedTimeLeft, OriginalTime);            
                        base.Client.SendWhisper("Te resta(n) " + base.Client.GetPlay().JailedTimeLeft + " minuto(s) para ser liberad@ de prisión.", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                #region Liberado #2
                if (base.Client.GetRoomUser() != null)
                {
                    Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                    string MyCity = Room.City;

                    PlayRoom Data;
                    int ToJail = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetJail(MyCity, out Data);
                    int ToJailback = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetJail(MyCity, out Data);

                    if (base.Client.GetHabbo().CurrentRoomId == ToJail || base.Client.GetHabbo().CurrentRoomId == ToJailback)//prision de la cd
                        RoleplayManager.SendUser(base.Client, ToJail, "");
                    //RoleplayManager.SpawnChairs(base.Client, "comodin_carr2");

                }

                base.Client.GetPlay().WantedFor = "";
                //base.Client.GetPlay().Trialled = false;
                //base.Client.GetPlay().Jailbroken = false;
                base.Client.GetPlay().InState = false;
                base.Client.GetPlay().IsJailed = false;
                base.Client.GetPlay().JailedTimeLeft = 0;
                base.Client.GetPlay().UpdateTimerDialogue("Jail-Timer", "remove", Client.GetPlay().JailedTimeLeft, OriginalTime);
                RoleplayManager.Shout(base.Client, "*Es liberado de prisión por cumplir su condena*", 5);
                UpdateJailLook();
                base.EndTimer();
                #endregion
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }

        private void UpdateJailLook()
        {
            base.Client.GetHabbo().Look = Client.GetPlay().OriginalOutfit;
            base.Client.GetHabbo().Motto = "Ciudadan@";
        }
    }
}