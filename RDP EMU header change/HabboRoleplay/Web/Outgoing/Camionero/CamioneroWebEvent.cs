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
    /// CamioneroWebEvent class.
    /// </summary>
    class CamioneroWebEvent : IWebEvent
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
                #region Open
                case "open":
                    {
                        #region Pagas
                        int Amn = 0, Med = 0, Crack = 0, Piezas = 0;

                        if (Client.GetPlay().CamLvl == 1)
                        {
                            Amn = 13;
                            Med = 2;
                            Crack = 1;
                            Piezas = 2;
                        }
                        else if (Client.GetPlay().CamLvl == 2)
                        {
                            Amn = 16;
                            Med = 4;
                            Crack = 2;
                            Piezas = 5;
                        }
                        else if (Client.GetPlay().CamLvl == 3)
                        {
                            Amn = 20;
                            Med = 6;
                            Crack = 3;
                            Piezas = 7;
                        }
                        else if (Client.GetPlay().CamLvl == 4)
                        {
                            Amn = 22;
                            Med = 8;
                            Crack = 4;
                            Piezas = 7;
                        }
                        else if (Client.GetPlay().CamLvl == 5)
                        {
                            Amn = 25;
                            Med = 10;
                            Crack = 5;
                            Piezas = 7;
                        }
                        else if (Client.GetPlay().CamLvl >= 6) // Max Lvl
                        {
                            Amn = 30;
                            Med = 12;
                            Crack = 6;
                            Piezas = 7;
                        }
                        #endregion

                        Client.GetPlay().ViewCamCargas = true;
                        Socket.Send("compose_camionero|open|" + Amn + "|" + Med + "|" + Crack + "|" + Piezas + "|");
                    }
                    break;
                #endregion

                #region Cargar
                case "cargar":
                    {
                        if (Client.GetPlay().TryGetCooldown("cargcam"))
                            return;

                        if (Client.GetRoomUser() == null || Client.GetRoomUser().GetRoom() == null)
                            return;

                        Room Room = Client.GetRoomUser().GetRoom();

                        if (Room == null)
                            return;

                        #region Conditions
                        if (RoleplayManager.PurgeEvent)
                        {
                            Socket.Send("compose_camionero|cammsg|¡No puedes trabajar durante la purga!");
                            return;
                        }
                        #endregion

                        #region Group Conditions
                        List<Groups.Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Client.GetHabbo().Id);

                        if (Groups.Count <= 0)
                        {
                            Socket.Send("compose_camionero|cammsg|No tienes ningún trabajo para hacer eso.");
                            return;
                        }

                        int GroupNumber = -1;

                        if (Groups[0].GType != 2)
                        {
                            if (Groups.Count > 1)
                            {
                                if (Groups[1].GType != 2)
                                {
                                    Socket.Send("compose_camionero|cammsg|((No perteneces a ningún trabajo usar ese comando))");
                                    return;
                                }
                                GroupNumber = 1; // Segundo indicie de variable
                            }
                            else
                            {
                                Socket.Send("compose_camionero|cammsg|((No perteneces a ningún trabajo para usar ese comando))");
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

                            Socket.Send("compose_camionero|cammsg|Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.");
                            return;
                        }

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "camionero"))
                        {
                            Socket.Send("compose_camionero|cammsg|Debes tener el trabajo de Camionero para usar ese comando.");
                            return;
                        }

                        if (!Client.GetPlay().DrivingCar)
                        {
                            Socket.Send("compose_camionero|cammsg|Debes conducir un Camión para hacer eso.");
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
                            Socket.Send("compose_camionero|cammsg|¡Ha ocurrido un error al buscar los datos del vehículo que conduces!");
                            return;
                        }
                        #endregion

                        if (!PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Camioneros"))
                        {
                            Socket.Send("compose_camionero|cammsg|Debes conducir un Camión para hacer eso.");
                            return;
                        }

                        string MyCity1 = Room.City;
                        int CamRoomID = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetCamioneros(MyCity1, out PlayRoom mData);//camioneros de la cd.
                        if (Client.GetHabbo().CurrentRoomId != CamRoomID)
                        {
                            Socket.Send("compose_camionero|cammsg|¡Debes estar en la zona de cargamento para Camioneros!");
                            return;
                        }
                        #endregion

                        #region Camionero Conditions
                        List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(Client.GetPlay().DrivingCarId);
                        if (VO == null || VO.Count <= 0)
                        {
                            Socket.Send("compose_camionero|cammsg|((No se pudo obtener información del vehículo que conduces))");
                            return;
                        }
                        if (VO[0].CamOwnId > 0 && VO[0].CamOwnId != Client.GetHabbo().Id)
                        {
                            Socket.Send("compose_camionero|cammsg|Este camión ya se encuentra cargado por otra persona.");
                            return;
                        }
                        // Para controlar que cargue solo un camion a la vez.
                        if (Client.GetPlay().CamCargId > 0)
                        {
                            Socket.Send("compose_camionero|cammsg|Ya has cargado un Camión. No puedes hacer más de un recorrido a la vez. Usa ':abandonarcarga' para comenzar uno nuevo.");
                            return;
                        }
                        if (VO[0].CamState > 0)
                        {
                            Socket.Send("compose_camionero|cammsg|Tu camión ya fue cargado. ¡Ve a entregar la carga a tu destino!");
                            return;
                        }

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Socket.Send("compose_camionero|cammsg|Debes estar en la zona de Cargamento para Cargar tu camión.");
                            return;
                        }
                        #endregion

                        if (Client.GetPlay().IsCamLoading)
                        {
                            Socket.Send("compose_camionero|cammsg|Ya te encuentras cargando el camión. Por favor espera...");
                            return;
                        }
                        #endregion

                        #region Execute
                        int ID;
                        string[] ReceivedData = Data.Split(',');
                        if (int.TryParse(ReceivedData[1], out ID))
                        {
                            if (ID < 1 || ID > 4)
                            {
                                Socket.Send("compose_camionero|cammsg|ID de carga inválida. Usa :cargas para ver un listado de ellas.");
                                return;
                            }

                            if (Client.GetPlay().PassiveMode && (ID == 3 || ID == 4))
                            {
                                Socket.Send("compose_camionero|cammsg|¡No puedes llevar cargamentos ilegales en modo pasivo!");
                                return;
                            }

                            if (RoleplayManager.getCamCargDest(Room, ID) < 1)
                            {
                                Client.SendNotification("Al parecer no hay destinos para entregar " + RoleplayManager.getCamCargName(ID) + " en esta Ciudad. ((Contacta con un Administrador))");
                                return;
                            }


                            VO[0].CamDest = RoleplayManager.getCamCargDest(Room, ID);
                            Client.GetPlay().CamCargId = ID;

                            // Timer
                            Client.GetPlay().IsCamLoading = true;
                            Client.GetPlay().LoadingTimeLeft = RoleplayManager.CamCargTime;

                            RoleplayManager.Shout(Client, "*Comienza a cargar su camión*", 5);
                            Client.SendWhisper("Debes esperar " + Client.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
                            Client.GetPlay().TimerManager.CreateTimer("general", 1000, true);
                            Client.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);

                            Client.GetPlay().ViewCamCargas = false;
                            Socket.Send("compose_camionero|close|");
                        }
                        else
                        {
                            Socket.Send("compose_camionero|cammsg|Ingresa una ID válida. ((:cargarcamion [ID]))");
                            return;
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Depositar
                case "depositar":
                    {
                        #region Conditions

                        if (Client.GetPlay().TryGetCooldown("cargcam"))
                            return;

                        if (Client.GetRoomUser() == null || Client.GetRoomUser().GetRoom() == null)
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

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "camionero"))
                        {
                            Client.SendWhisper("Debes tener el trabajo de Camionero para usar ese comando.", 1);
                            return;
                        }
                        if (!Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("Debes conducir un Camión para hacer eso.", 1);
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

                        if (!Client.GetPlay().DrivingCar || !PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Camioneros"))
                        {
                            Client.SendWhisper("Debes conducir un Camión para hacer eso.", 1);
                            return;
                        }

                        #endregion

                        #region Camionero Conditions
                        List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(Client.GetPlay().DrivingCarId);
                        if (VO == null || VO.Count <= 0)
                        {
                            Client.SendWhisper("((No se pudo obtener información del vehículo que conduces))", 1);
                            return;
                        }
                        if (VO[0].CamOwnId > 0 && VO[0].CamOwnId != Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("Este camión no ha sido cargado bajo tu nombre. No puedes hacer recorridos ajenos.", 1);
                            return;
                        }
                        if (VO[0].CamState != 1)
                        {
                            Client.SendWhisper("El camión no ha sido cargado aún.", 1);
                            return;
                        }
                        if (VO[0].CamState == 2)
                        {
                            Client.SendWhisper("El camión ya ha sido descargado. ¡Ve a entregarlo a Camioneros! ((Usa :entregarcamion))", 1);
                            return;
                        }
                        if (VO[0].CamDest != Room.Id)
                        {
                            Room _room = RoleplayManager.GenerateRoom(VO[0].CamDest);
                            Client.SendWhisper("¡Debes ir a " + _room.Name + " para entregar la mercancía!", 1);
                            return;
                        }

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carro" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes estar en la zona de descarga de tu destino para entregar la mercancía.", 1);
                            return;
                        }
                        #endregion

                        if (Client.GetPlay().IsCamUnLoading)
                        {
                            Client.SendWhisper("Ya te encuentras descargando el camión. Por favor espera...", 1);
                            return;
                        }
                        #endregion

                        #region Execute
                        Client.GetPlay().IsCamUnLoading = true;
                        Client.GetPlay().LoadingTimeLeft = RoleplayManager.CamDepositTime;

                        RoleplayManager.Shout(Client, "*Comienza a descargar su camión*", 5);
                        Client.SendWhisper("Debes esperar " + Client.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
                        Client.GetPlay().TimerManager.CreateTimer("general", 1000, true);
                        Client.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
                        #endregion
                    }
                    break;
                #endregion

                #region Entregar
                case "entregar":
                    {
                        #region Conditions
                        if (Client.GetPlay().TryGetCooldown("cargcam"))
                            return;

                        if (Client.GetRoomUser() == null || Client.GetRoomUser().GetRoom() == null)
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

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "camionero"))
                        {
                            Client.SendWhisper("Debes tener el trabajo de Camionero para usar ese comando.", 1);
                            return;
                        }
                        if (!Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("Debes conducir un Camión para hacer eso.", 1);
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

                        if (!Client.GetPlay().DrivingCar || !PlusEnvironment.GetGame().GetGroupManager().GetJob(corp).Name.Contains("Camioneros"))
                        {
                            Client.SendWhisper("Debes conducir un Camión para hacer eso.", 1);
                            return;
                        }
                        #endregion

                        #region Camionero Conditions
                        List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(Client.GetPlay().DrivingCarId);
                        if (VO == null || VO.Count <= 0)
                        {
                            Client.SendWhisper("((No se pudo obtener información del vehículo que conduces))", 1);
                            return;
                        }
                        if (VO[0].CamOwnId > 0 && VO[0].CamOwnId != Client.GetHabbo().Id)
                        {
                            Client.SendWhisper("Este camión pertenece a por otra persona. ((Si perdiste el tuyo usa :abandonarcarga))", 1);
                            return;
                        }
                        if (VO[0].CamState == 0)
                        {
                            Client.SendWhisper("El camión no ha sido cargado aún. ¡Ve a cargarlo de mercancía! ((Usa :cargarcamion [ID]))", 1);
                            return;
                        }
                        if (VO[0].CamState != 2)
                        {
                            Client.SendWhisper("El camión no ha sido descargado aún. ¡Ve a entregar la mercancía!", 1);
                            return;
                        }
                        if (VO[0].CamDest != Room.Id)
                        {
                            Room _room = RoleplayManager.GenerateRoom(VO[0].CamDest);
                            Client.SendWhisper("¡Debes ir a " + _room.Name + " para entregar el camión!", 1);
                            return;
                        }

                        #region Comodin Conditions
                        Item BTile = null;
                        BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "comodin_carr2" && x.Coordinate == Client.GetRoomUser().Coordinate);
                        if (BTile == null)
                        {
                            Client.SendWhisper("Debes estar en la zona de entrega para terminar el recorrido.", 1);
                            return;
                        }
                        #endregion

                        #endregion

                        #region Execute

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

                        List<Groups.Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Client.GetHabbo().Id);

                        #region Pagas
                        int Amn = 0, Med = 0, Crack = 0, Piezas = 0, bonif = 0;

                        #region Cants By Level
                        if (Client.GetPlay().CamLvl == 1)
                        {
                            Amn = 13;
                            Med = 1;
                            Crack = 1;
                            Piezas = 1;
                        }
                        else if (Client.GetPlay().CamLvl == 2)
                        {
                            Amn = 16;
                            Med = 3;
                            Crack = 2;
                            Piezas = 2;
                        }
                        else if (Client.GetPlay().CamLvl == 3)
                        {
                            Amn = 20;
                            Med = 4;
                            Crack = 3;
                            Piezas = 4;
                        }
                        else if (Client.GetPlay().CamLvl == 4)
                        {
                            Amn = 22;
                            Med = 5;
                            Crack = 4;
                            Piezas = 5;
                        }
                        else if (Client.GetPlay().CamLvl == 5)
                        {
                            Amn = 25;
                            Med = 7;
                            Crack = 5;
                            Piezas = 5;
                        }
                        else if (Client.GetPlay().CamLvl >= 6) // Max Lvl
                        {
                            Amn = 30;
                            Med = 10;
                            Crack = 6;
                            Piezas = 7;
                        }
                        #endregion

                        string win = "¡Excelente entrega! Tus ganancias son: $" + Amn;
                        #region By Carg
                        if (VO[0].CamCargId == 3)// Drogas
                        {
                            Amn = 0;
                            Client.GetPlay().Cocaine += Crack;
                            Client.GetPlay().Medicines += Med;
                            Client.GetPlay().CocaineTaken += Crack;
                            Client.GetPlay().MedicinesTaken += Med;
                            //RoleplayManager.SaveQuickStat(Client, "cocaine", "" + Client.GetPlay().Cocaine);
                            //RoleplayManager.SaveQuickStat(Client, "medicines", "" + Client.GetPlay().Medicines);
                            win = "¡Excelente entrega! Tus ganancias son: " + Med + " Medicamentos + " + Crack + "g de Crack.";

                            #region Gang Bonif
                            if (MyGang != null && MyGang.Count > 0 && !MyGang[0].BankRuptcy)
                            {
                                int NewTurfsCount = 0;
                                List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(MyGang[0].Id);
                                if (TF != null && TF.Count > 0)
                                    NewTurfsCount = TF.Count;

                                bonif = RoleplayManager.GangsTurfBonif * NewTurfsCount;
                                if (bonif > 0)
                                {
                                    MyGang[0].GangFarmCocaine += Crack;
                                    MyGang[0].GangFarmMedicines += Med;
                                    MyGang[0].Bank += bonif;
                                    MyGang[0].UpdateStat("gang_farm_cocaine", MyGang[0].GangFarmCocaine);
                                    MyGang[0].UpdateStat("gang_farm_medicines", MyGang[0].GangFarmMedicines);
                                    MyGang[0].SetBussines(MyGang[0].Bank, MyGang[0].Stock);

                                    Client.GetHabbo().Credits += bonif;
                                    Client.GetPlay().MoneyEarned += bonif;
                                    Client.GetHabbo().UpdateCreditsBalance();
                                }
                            }
                            #endregion
                        }
                        else if (VO[0].CamCargId == 4)// Armas
                        {
                            Amn = 0;
                            Client.GetPlay().ArmPieces += Piezas;
                            //RoleplayManager.SaveQuickStat(Client, "Pieces", "" + Client.GetPlay().Pieces);
                            win = "¡Excelente entrega! Tus ganancias son: " + Piezas + " Piezas de armas.";

                            #region Gang Bonif
                            if (MyGang != null && MyGang.Count > 0 && !MyGang[0].BankRuptcy)
                            {
                                int NewTurfsCount = 0;
                                List<GangTurfs> TF = PlusEnvironment.GetGame().GetGangTurfsManager().getTurfsbyGang(MyGang[0].Id);
                                if (TF != null && TF.Count > 0)
                                    NewTurfsCount = TF.Count;

                                bonif = RoleplayManager.GangsTurfBonif * NewTurfsCount;
                                if (bonif > 0)
                                {
                                    MyGang[0].Bank += bonif;
                                    MyGang[0].SetBussines(MyGang[0].Bank, MyGang[0].Stock);

                                    Client.GetHabbo().Credits += bonif;
                                    Client.GetPlay().MoneyEarned += bonif;
                                    Client.GetHabbo().UpdateCreditsBalance();
                                }
                            }
                            #endregion
                        }

                        Client.GetHabbo().Credits += Amn;                        
                        Client.GetPlay().MoneyEarned += Amn;
                        Client.GetHabbo().UpdateCreditsBalance();
                        
                        #endregion
                        #endregion

                        // Reseteamos Camion por seguridad
                        VO[0].CamCargId = 0;
                        VO[0].CamDest = 0;
                        VO[0].CamOwnId = 0;
                        VO[0].CamState = 0;

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

                        Client.GetPlay().CamCargId = 0;// Retornamos a 0 para que pueda cargar otro

                        RoleplayManager.Shout(Client, "*Entrega su camión completando su recorrido*", 5);
                        Client.SendWhisper(win, 1);
                        RoleplayManager.JobSkills(Client, Client.GetPlay().JobId, Client.GetPlay().CamLvl, Client.GetPlay().CamXP);

                        if (MyGang != null && MyGang.Count > 0)
                        {
                            if (MyGang[0].BankRuptcy) {
                                Client.SendWhisper("Tu banda está en bancarota y no podrás gozar de los beneficios de ella.", 1);
                            }
                            else if (bonif > 0)
                            {
                                MyGang[0].AddLog(Client.GetHabbo().Id, Client.GetHabbo().Username + " ha obtenido $ " + String.Format("{0:N0}", bonif) + " para la banda en cargas ilegales.", bonif);
                                Client.SendWhisper("¡Tu banda y tú han ganado una bonifcación extra de $ " + String.Format("{0:N0}", bonif) + " por tu entrega ilegal!", 1);
                            }                                
                        }

                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_vehicle", "close");// WS FUEL
                        Client.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
                        #endregion
                    }
                    break;
                #endregion

                #region Abandonar
                case "abandonar":
                    {
                        #region Conditions
                        if (Client.GetPlay().TryGetCooldown("cargcam"))
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

                        if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "camionero") && !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "basurero"))
                        {
                            Client.SendWhisper("Debes tener el trabajo de Camionero o Basurero para usar ese comando.", 1);
                            return;
                        }
                        /*
                        if (Client.GetPlay().DrivingCar)
                        {
                            Client.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                            return;
                        }
                        */
                        #endregion

                        #region Camionero Conditions
                        if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Client, "camionero"))
                        {
                            List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByCamOwnId(Client.GetHabbo().Id);
                            if (VO == null || VO.Count <= 0)
                            {
                                Client.SendWhisper("No tienes ninguna carga a tu nombre para abandonar.", 1);
                                return;
                            }
                            else
                            {
                                // Quitar Camión de Diccionario
                                PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(VO[0].Id);
                                Client.GetPlay().CamCargId = 0;
                            }
                        }
                        else
                        {
                            if (!Client.GetPlay().IsBasuChofer)
                            {
                                Client.SendWhisper("No eres el chofer de ninguna carga de basura a abandonar.", 1);
                                return;
                            }
                        }
                        #endregion

                        #region Execute
                        RoleplayManager.Shout(Client, "*Abandona la Carga de su Camión*", 5);

                        if (!Client.GetPlay().IsBasuChofer)
                            Client.SendWhisper("Tu Camión ha sido descargadado. No has terminado el recorrido, no se te pagará nada.", 1);
                        else
                        {
                            Client.GetPlay().IsBasuChofer = false;

                            // Solo al abandonar carga
                            Client.GetPlay().BasuTeamId = 0;
                            Client.GetPlay().BasuTeamName = string.Empty;
                            Client.GetPlay().BasuTrashCount = 0;

                            Client.SendWhisper("Su Camión ha sido abandonado. No han terminado el recorrido, no se les pagará nada.", 1);
                        }

                        RoleplayManager.CheckCorpCarp(Client);

                        #region Driving
                        if (Client.GetPlay().DrivingCar)
                        {
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
                                    {
                                        PJ.GetPlay().IsBasuPasaj = false;

                                        // Solo al abandonar carga
                                        PJ.GetPlay().BasuTeamId = 0;
                                        PJ.GetPlay().BasuTeamName = string.Empty;
                                        PJ.GetPlay().BasuTrashCount = 0;
                                        PJ.SendWhisper("Su Camión ha sido abandonado. No han terminado el recorrido, no se les pagará nada.", 1);
                                    }

                                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(PJ, "event_vehicle", "close");// WS FUEL
                                }
                            }

                            // CHOFER 
                            Client.GetPlay().PasajerosCount = 0;
                            Client.GetPlay().Pasajeros = "";
                            Client.GetPlay().Chofer = false;

                            if (Client.GetRoomUser() != null)
                                Client.GetRoomUser().AllowOverride = false;
                            #endregion

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

                            if (Client.GetRoomUser() != null)
                            {
                                Client.GetRoomUser().ApplyEffect(0);
                                Client.GetRoomUser().FastWalking = false;
                            }
                            #endregion

                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_vehicle", "close");// WS FUEL
                        }
                        #endregion

                        Client.GetPlay().CooldownManager.CreateCooldown("cargcam", 1000, 5);
                        #endregion
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetPlay().ViewCamCargas = false;
                        Socket.Send("compose_camionero|close|");
                    }
                    break;
                #endregion
            }
        }
    }
}
