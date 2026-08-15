using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Incoming.Groups;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Cache;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using System.Text.RegularExpressions;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboRoleplay.PhoneChat;
using System.Data;
using Plus.HabboHotel.Users.Messenger;
using Plus.Utilities;
using Plus.HabboHotel.Quests;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboRoleplay.Phones;
using Plus.HabboRoleplay.PhoneOwned;
using Plus.HabboRoleplay.PhoneAppOwned;
using Plus.HabboRoleplay.PhonesApps;
using System.Web;
using Plus.Communication.Packets.Incoming.Inventory.Purse;
using Plus.HabboRoleplay.API;
using Plus.HabboHotel.RolePlay.PlayInternet;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.GangTurfs;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// BasureroWebEvent class.
    /// </summary>
    class BasureroWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;
            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);


            switch (Action)
            {
                #region Entregar
                case "descargar":
                    {
                        #region Conditions
                        if (Client.GetPlay().TryGetCooldown("cargcam"))
                            return;

                        Room Room = Client.GetRoomUser().GetRoom();

                        if (Room == null)
                            return;
                        #endregion

                        #region Group Conditions
                        List<Groups.Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Client.GetHabbo().Id);

                        if (Groups.Count <= 0)
                        {
                            Client.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                            return;
                        }

                        int GroupNumber = -1;

                        if (Groups[0].GType != 2)
                        {
                            if (Groups.Count > 1)
                            {
                                if (Groups[1].GType != 2)
                                {
                                    Client.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                                    return;
                                }
                                GroupNumber = 1; // Segundo indicie de variable
                            }
                            else
                            {
                                Client.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
                                return;
                            }
                        }
                        else
                        {
                            GroupNumber = 0; // Primer indice de Variable Group
                        }

                        Client.GetPlay().JobId = Groups[GroupNumber].Id;
                        Client.GetPlay().JobRank = Groups[GroupNumber].Members[Client.GetHabbo().Id].UserRank;
                        #endregion

                        #region Extra Conditions            
                        // Existe el trabajo?
                        if (!PlusEnvironment.GetGame().GetGroupManager().JobExists(Client.GetPlay().JobId, Client.GetPlay().JobRank))
                        {
                            Client.GetPlay().TimeWorked = 0;
                            Client.GetPlay().JobId = 0; // Desempleado
                            Client.GetPlay().JobRank = 0;

                            //Room.Group.DeleteMember(Client.GetHabbo().Id);// OJO ACÁ

                            Client.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                            return;
                        }

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "basurero"))
                        {
                            Client.SendWhisper("Debes tener el trabajo de Basurero para usar ese comando.", 1);
                            return;
                        }

                        #endregion

                        #region Basurero Conditions
                        string MyCity = Room.City;
                        int Basurero = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetBasureros(MyCity, out PlayRoom mData);
                        if (Room.Id != Basurero)
                        {
                            Client.SendWhisper("¡Debes ir al Basurero de la Ciudad para descargar el camión!", 1);
                            return;
                        }
                        if (Client.GetPlay().BasuTeamId <= 0)
                        {
                            Client.SendWhisper("¡Primero debes conseguir un compañero de trabajo para recolectar 15 contenedores de basura!", 1);
                            return;
                        }
                        if (Client.GetPlay().DrivingInCar)
                        {
                            Client.SendWhisper("¡Debes estar conduciendo el camión de basura!", 1);
                            return;
                        }
                        #region Get Information form VehiclesManager
                        Vehicle vehicle = null;
                        int corp = 0;
                        foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                        {
                            if (Client.GetPlay().CarEffectId == Vehicle.EffectID)
                            {
                                vehicle = Vehicle;
                                corp = Convert.ToInt32(Vehicle.CarCorp);
                            }
                        }
                        if (vehicle == null)
                        {
                            Client.SendWhisper("¡Ha ocurrido un error al buscar los datos del vehículo que conduces!", 1);
                            return;
                        }
                        #endregion

                        if (corp != Client.GetPlay().JobId)
                        {
                            Client.SendWhisper("¡Debes estar conduciendo el camión de basura!", 1);
                            return;
                        }
                        if (!Client.GetPlay().IsBasuChofer)
                        {
                            Client.SendWhisper("¡Solo el chofer del Camión puede hacer eso!", 1);
                            return;
                        }
                        if (Client.GetPlay().BasuTrashCount < 15)
                        {
                            Client.SendWhisper("¡Deben recolectar 15 contedendores de basura para poder descargar el camión!", 1);
                            return;
                        }
                        GameClient TeamPasaj = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Client.GetPlay().BasuTeamId);
                        if (TeamPasaj == null)
                        {
                            Client.SendWhisper("Al parecer tu compañero de Basurero se ha ido y han Fracasado el Recorrido.", 1);

                            // Quitar Camión
                            PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                            #region Online ParkVars
                            //Retornamos a valores predeterminados
                            Client.GetPlay().DrivingCar = false;
                            Client.GetPlay().DrivingInCar = false;
                            Client.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                            //Combustible System
                            Client.GetPlay().CarType = 0;// Define el gasto de combustible
                            Client.GetPlay().CarFuel = 0;
                            Client.GetPlay().CarMaxFuel = 0;
                            Client.GetPlay().CarTimer = 0;
                            Client.GetPlay().CarLife = 0;

                            Client.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                            Client.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                            Client.GetRoomUser().ApplyEffect(0);
                            Client.GetRoomUser().FastWalking = false;
                            #endregion

                            #region Retornamos Basurero Vars
                            Client.GetPlay().BasuTeamId = 0;
                            Client.GetPlay().BasuTeamName = "";
                            Client.GetPlay().BasuTrashCount = 0;
                            Client.GetPlay().IsBasuPasaj = false;
                            Client.GetPlay().IsBasuChofer = false;
                            #endregion
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_vehicle", "close");// WS FUEL
                            return;
                        }
                        if (!Client.GetPlay().Pasajeros.Contains(TeamPasaj.GetHabbo().Username))
                        {
                            Client.SendWhisper("Tu compañero de Basurero debe estar de Pasajero de tu Camión.", 1);
                            return;
                        }

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes estar en cerca del bulto de basura central para terminar el recorrido.", 1);
                            return;
                        }
                        #endregion

                        #endregion

                        #region Execute

                        #region Pagas
                        int PayC = (Client.GetPlay().BasuLvl * 25);
                        int PayP = (TeamPasaj.GetPlay().BasuLvl * 25);
                        #endregion

                        // Quitar Camión
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                        RoleplayManager.CheckCorpCarp(Client);

                        #region Online ParkVars
                        //Retornamos a valores predeterminados
                        Client.GetPlay().DrivingCar = false;
                        Client.GetPlay().DrivingInCar = false;
                        Client.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                        //Combustible System
                        Client.GetPlay().CarType = 0;// Define el gasto de combustible
                        Client.GetPlay().CarFuel = 0;
                        Client.GetPlay().CarMaxFuel = 0;
                        Client.GetPlay().CarTimer = 0;
                        Client.GetPlay().CarLife = 0;

                        Client.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                        Client.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                        Client.GetRoomUser().ApplyEffect(0);
                        Client.GetRoomUser().FastWalking = false;
                        #endregion

                        #region Retornamos Basurero Vars
                        Client.GetPlay().BasuTeamId = 0;
                        Client.GetPlay().BasuTeamName = "";
                        Client.GetPlay().BasuTrashCount = 0;
                        Client.GetPlay().IsBasuPasaj = false;
                        Client.GetPlay().IsBasuChofer = false;

                        TeamPasaj.GetPlay().BasuTeamId = 0;
                        TeamPasaj.GetPlay().BasuTeamName = "";
                        TeamPasaj.GetPlay().BasuTrashCount = 0;
                        TeamPasaj.GetPlay().IsBasuPasaj = false;
                        TeamPasaj.GetPlay().IsBasuChofer = false;
                        #endregion

                        #region Online ParkVars & Pasajero TeamPasaj
                        //Retornamos a valores predeterminados
                        TeamPasaj.GetPlay().DrivingCar = false;
                        TeamPasaj.GetPlay().DrivingInCar = false;
                        TeamPasaj.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                        //Combustible System
                        TeamPasaj.GetPlay().CarType = 0;// Define el gasto de combustible
                        TeamPasaj.GetPlay().CarFuel = 0;
                        TeamPasaj.GetPlay().CarMaxFuel = 0;
                        TeamPasaj.GetPlay().CarTimer = 0;
                        TeamPasaj.GetPlay().CarLife = 0;

                        TeamPasaj.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                        TeamPasaj.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                        if (TeamPasaj.GetRoomUser() != null)
                            TeamPasaj.GetRoomUser().ApplyEffect(0);
                        TeamPasaj.GetRoomUser().FastWalking = false;

                        // PASAJERO
                        TeamPasaj.GetPlay().Pasajero = false;
                        TeamPasaj.GetPlay().ChoferName = "";
                        TeamPasaj.GetPlay().ChoferID = 0;
                        if (TeamPasaj.GetRoomUser() != null)
                        {
                            TeamPasaj.GetRoomUser().CanWalk = true;
                            TeamPasaj.GetRoomUser().FastWalking = false;
                            TeamPasaj.GetRoomUser().TeleportEnabled = false;
                            TeamPasaj.GetRoomUser().AllowOverride = false;
                        }

                        // CHOFER 
                        // Descontamos Pasajero
                        Client.GetPlay().PasajerosCount = 0;
                        Client.GetPlay().Pasajeros = "";
                        Client.GetPlay().Chofer = false;
                        Client.GetRoomUser().AllowOverride = false;
                        #endregion

                        #region Retornamos Basurero Vars TeamPasaj
                        TeamPasaj.GetPlay().BasuTeamId = 0;
                        TeamPasaj.GetPlay().BasuTeamName = "";
                        TeamPasaj.GetPlay().BasuTrashCount = 0;
                        TeamPasaj.GetPlay().IsBasuPasaj = false;
                        TeamPasaj.GetPlay().IsBasuChofer = false;

                        TeamPasaj.GetPlay().BasuTeamId = 0;
                        TeamPasaj.GetPlay().BasuTeamName = "";
                        TeamPasaj.GetPlay().BasuTrashCount = 0;
                        TeamPasaj.GetPlay().IsBasuPasaj = false;
                        TeamPasaj.GetPlay().IsBasuChofer = false;
                        #endregion

                        #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                        //Vars
                        string Pasajeros = Client.GetPlay().Pasajeros;
                        string[] stringSeparators = new string[] { ";" };
                        string[] result;
                        result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string psjs in result)
                        {
                            GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                            if (PJ != null && PJ.GetPlay() != null && PJ.GetRoomUser() != null)
                            {
                                if (PJ.GetPlay().ChoferName == Client.GetHabbo().Username)
                                {
                                    RoleplayManager.Shout(PJ, "*Baja del vehículo de " + Client.GetHabbo().Username + "*", 5);
                                }
                                // PASAJERO
                                PJ.GetPlay().Pasajero = false;
                                PJ.GetPlay().ChoferName = "";
                                PJ.GetPlay().ChoferID = 0;
                                PJ.GetRoomUser().CanWalk = true;
                                PJ.GetRoomUser().FastWalking = false;
                                PJ.GetRoomUser().TeleportEnabled = false;
                                PJ.GetRoomUser().AllowOverride = false;

                                // SI EL PASAJERO ES UN HERIDO
                                if (PJ.GetPlay().IsDying)
                                {
                                    #region Send To Hospital
                                    RoleplayManager.Shout(PJ, "*Pierde el conocimiento y es trasladad@ al hospital*", 5);
                                    Room Room2 = RoleplayManager.GenerateRoom(PJ.GetRoomUser().RoomId);
                                    string MyCity2 = Room2.City;

                                    PlayRoom Data2;
                                    int ToHosp2 = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity2, out Data2);

                                    if (ToHosp2 > 0)
                                    {
                                        Room Room3 = RoleplayManager.GenerateRoom(ToHosp2);
                                        if (Room3 != null)
                                        {
                                            PJ.GetPlay().IsDead = true;
                                            PJ.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;
                                            PJ.GetHabbo().HomeRoom = ToHosp2;
                                            RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                                        }
                                        else
                                        {
                                            PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                            PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                            PJ.GetPlay().RefreshStatDialogue();
                                            PJ.GetRoomUser().Frozen = false;
                                            PJ.GetRoomUser().CanWalk = true;
                                            PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                        }
                                    }
                                    else
                                    {
                                        PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                        PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                        PJ.GetPlay().RefreshStatDialogue();
                                        PJ.GetRoomUser().Frozen = false;
                                        PJ.GetRoomUser().CanWalk = true;
                                        PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                    }

                                    PJ.GetPlay().IsDying = false;
                                    PJ.GetPlay().DyingTimeLeft = 0;
                                    #endregion
                                }

                                // SI EL PASAJERO ES COMPAÑERO DE BASURERO
                                if (PJ.GetPlay().IsBasuPasaj)
                                    PJ.GetPlay().IsBasuPasaj = false;

                                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                            }
                        }

                        // CHOFER 
                        Client.GetPlay().PasajerosCount = 0;
                        Client.GetPlay().Pasajeros = "";
                        Client.GetPlay().Chofer = false;
                        Client.GetRoomUser().AllowOverride = false;
                        #endregion

                        RoleplayManager.Shout(Client, "*Descarga el camión de basura completando su trabajo junto con " + TeamPasaj.GetHabbo().Username + "*", 5);
                        Client.SendWhisper("¡Buen trabajo! Tus ganancias son: " + PayC, 1);
                        TeamPasaj.SendWhisper("¡Buen trabajo! Tus ganancias son: " + PayP, 1);

                        Client.GetHabbo().Credits += PayC;
                        Client.GetPlay().MoneyEarned += PayC;
                        Client.GetHabbo().UpdateCreditsBalance();
                        TeamPasaj.GetHabbo().Credits += PayP;
                        TeamPasaj.GetPlay().MoneyEarned += PayP;
                        TeamPasaj.GetHabbo().UpdateCreditsBalance();

                        RoleplayManager.JobSkills(Client, Client.GetPlay().JobId, Client.GetPlay().BasuLvl, Client.GetPlay().BasuXP);
                        RoleplayManager.JobSkills(TeamPasaj, TeamPasaj.GetPlay().JobId, TeamPasaj.GetPlay().BasuLvl, TeamPasaj.GetPlay().BasuXP);

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_vehicle", "close");// WS FUEL
                        Client.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
                        return;
                        #endregion
                    }
                    break;
                    #endregion
            }
        }
    }
}
