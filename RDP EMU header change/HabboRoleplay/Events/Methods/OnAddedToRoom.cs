using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.Houses;

namespace Plus.HabboRoleplay.Events.Methods
{
    /// <summary>
    /// Triggered when the user is added to the room
    /// </summary>
    public class OnAddedToRoom : IEvent
    {
        #region Execute Event
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;
            if (Client == null || Client.GetPlay() == null || Client.GetHabbo() == null)
                return;

            Room Room = (Room)Params[0];

            #region WebSocket Dialogue Check
            Client.GetPlay().ClearWebSocketDialogue();
            #endregion

            #region Spawn/Update House Signs OFF
            /*
            List<House> Houses = PlusEnvironment.GetGame().GetHouseManager().GetHousesBySignRoomId(Room.Id);
            if (Houses.Count > 0)
            {
                foreach (House House in Houses)
                {
                    if (House.Sign.Spawned)
                    {
                        if (House.Sign.Item.GetX != House.Sign.X && House.Sign.Item.GetY != House.Sign.Y && House.Sign.Item.GetZ != House.Sign.Z)
                            House.SpawnSign();
                    }
                    else
                        House.SpawnSign();
                }
            }
            */
            #endregion

            #region Room Entrance Message
            if (Room.EnterRoomMessage != "none")
            {
                new Thread(() =>
                {
                    Thread.Sleep(500);
                    Client.SendWhisper(Room.EnterRoomMessage, 34);
                }).Start();
            }
            #endregion
            #region Main checks
            HomeRoomCheck(Client, Params);
            //JobCheck(Client, Params);
            WantedCheck(Client, Params);
            // Checkings in GetRoomEntryDataEvent.cs
            //DeathCheck(Client, Params);
            //DyingCheck(Client, Params);
            //JailCheck(Client, Params);
            #region AFK check

            if (Client.GetRoomUser() != null)
                Client.GetHabbo().Poof(true);

            #endregion

            #endregion
        }
        #endregion

        #region HomeRoomCheck
        /// <summary>
        /// Checks if the users homeroom is the correct one
        /// </summary>
        private void HomeRoomCheck(GameClient Client, object[] Params)
        {
            Room Room = (Room)Params[0];
            if (Room.Id == 0)
                return;

            if (Client.GetHabbo().HomeRoom != Room.Id)
                Client.GetHabbo().HomeRoom = Room.Id;
        }
        #endregion

        #region DeathCheck
        /// <summary>
        /// Checks to see if the client is dead, if true send back to hospital if not already in one
        /// </summary>
        ///
        /*
        private void DeathCheck(GameClient Client, object[] Params)
        {
            if (Client.GetPlay().IsDead)
            {
                Room Room = (Room)Params[0];
                if (Room.Id == 0)
                    return;

                string MyCity = Room.City;

                PlayRoom Data;
                int ToRoomId = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);

                if (Room.Id != ToRoomId)
                {
                    RoleplayManager.SendUserTimer(Client, ToRoomId, "¡No puedes abandonar el Hospital sin haber sido dad@ de alta!", "death");
                }
            }
            else
                return;
        }
        */
        #endregion

        #region DyingCheck
        /*
        private void DyingCheck(GameClient Client, object[] Params)
        {
            if (Client.GetPlay().IsDying)
            {
                Room Room = (Room)Params[0];
                if (Room.Id == 0)
                    return;

                if (Room.Id != Client.GetHabbo().HomeRoom)
                {
                    RoleplayManager.SendUserTimer(Client, Client.GetHabbo().HomeRoom, "¡No puedes ir a ningún lado en tu estado actal!", "dying");
                }
            }
        }
        */
        #endregion

        #region JobCheck
        private void JobCheck(GameClient Client, object[] Params)
        {
            if (Client.GetHabbo().CurrentRoom == null)
                Client.GetPlay().IsWorking = false;

            if (Client.GetPlay().JobId > 0 && Client.GetPlay().IsWorking)
            {
                Room Room = (Room)Params[0];
                int JobId = Client.GetPlay().JobId;
                int JobRank = Client.GetPlay().JobRank;

                if (!PlusEnvironment.GetGame().GetGroupManager().GetJobRank(JobId, JobRank).CanWorkHere(Room.Id))
                {
                    WorkManager.RemoveWorkerFromList(Client);
                    Client.GetPlay().IsWorking = false;
                    Client.GetHabbo().Poof();
                    RoleplayManager.Shout(Client, "*Ha dejado de trabajar por salir de su zona de trabajo", 5);
                    Client.SendNotification("Has dejado de trabajar por abandonar tu zona de trabajo.");
                }
            }
            else
                return;
        }
        #endregion

        #region Wanted Check
        /// <summary>
        /// Checks if the user is wanted
        /// </summary>
        /// <param name="Client"></param>
        public void WantedCheck(GameClient Client, object[] Params)
        {
            if (!Client.GetPlay().IsWanted)
                return;

            if (RoleplayManager.WantedList.ContainsKey(Client.GetHabbo().Id))
                return;

            Room Room = (Room)Params[0];
            string RoomId = Room.Id.ToString() != "0" ? Room.Id.ToString() : "Unknown";

            if (!RoleplayManager.WantedList.ContainsKey(Client.GetHabbo().Id))
            {
                Wanted Wanted = new Wanted(Convert.ToUInt32(Client.GetHabbo().Id), RoomId, Client.GetPlay().WantedLevel);
                RoleplayManager.WantedList.TryAdd(Client.GetHabbo().Id, Wanted);
            }

            if (!Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("wanted"))
                Client.GetPlay().TimerManager.CreateTimer("wanted", 1000, false);
        }
        #endregion

    }
}