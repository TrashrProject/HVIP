using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Database.Interfaces;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using System.Data;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.VehiclesJobs;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Self
{
    class DrivingCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_driving_driving"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite conducir un vehículo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡Ya te encuentras conduciendo un vehículo!", 1);
                return;
            }
            if (Session.GetPlay().DrivingInCar)
            {
                Session.SendWhisper("No puedes hacer eso mientras vas de pasajer@.", 1);
                return;
            }
            if (Session.GetPlay().IsDying || Session.GetPlay().IsDead)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás muert@.", 1);
                return;
            }
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@.", 1);
                return;
            }
            if (Session.GetHabbo().EscortID > 0)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás siendo escoltad@", 1);
                return;
            }
            if (Session.GetHabbo().TaxiChofer > 0)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás vas en Taxi", 1);
                return;
            }
            if (Session.GetHabbo().Escorting > 0)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás escoltas a alguien", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk || Session.GetRoomUser().Frozen)
			{
                Session.SendWhisper("No puedes ni moverte como para poder conducir.", 1);
                return;
            }
            if (!Room.DriveEnabled)
            {
                Session.SendWhisper("¡No es posible conducir en esta zona!", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("car"))
                return;
            #endregion

            #region Execute

            #region Get Veihcle Info (play_vehicles)  
            Vehicle vehicle = null;
            bool found = false;
            int itemfurni = 0, corp = 0;
            Item BTile = null;
            string itemnm = null;
            foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
            {
                if (!found)
                {
                    BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == Vehicle.ItemName && x.Coordinate == Session.GetRoomUser().Coordinate);
                    if (BTile != null)
                    {
                        vehicle = Vehicle;
                        itemfurni = BTile.Id;
                        itemnm = Vehicle.ItemName;
                        corp = Convert.ToInt32(Vehicle.CarCorp);
                        found = true;
                    }
                }
            }
            //Al examinar todos los autos ninguno conincide con el item donde está parado el user...
            if (!found)
            {
                Session.SendWhisper("¡Debes estar sobre un vehículo para conducir!", 1);
                return;
            }
            #endregion

            Group JobInfo = null;
            #region Corp > 0 then Valid my Job
            if (corp > 0)
            {
                if (RoleplayManager.PurgeEvent)
                {
                    Session.SendWhisper("¡No puedes usar vehículos de trabajo durante la purga!", 1);
                    return;
                }

                // Set GetPlay().JobId
                #region Group Conditions
                List<Group> Groups = PlusEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

                if (Groups.Count <= 0)
                {
                    Session.SendWhisper("No tienes ningún trabajo para conducir este vehículo.", 1);
                    return;
                }

                PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(corp, out JobInfo);

                int GroupNumber = -1;
                int count = 0;
                foreach(var GPO in Groups)
                {
                    if (corp == GPO.Id)
                        GroupNumber = count;

                    count++;
                }

                if (JobInfo != null)
                {
                    if (GroupNumber <= -1)
                    {
                        Session.SendWhisper("No perteneces al trabajo de " + JobInfo.Name + " para conducir este vehículo.", 1);
                        return;
                    }
                }

                Session.GetPlay().JobId = Groups[GroupNumber].Id;
                Session.GetPlay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
                #endregion

                #region Job Conditions
                PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(corp, out JobInfo);
                if (JobInfo != null)
                {
                    if (Session.GetPlay().JobId != corp)
                    {
                        Session.SendWhisper("No perteneces al trabajo de " + JobInfo.Name + " para conducir este vehículo.", 1);
                        return;
                    }
                    if(vehicle.DisplayName == "Camión VIP")
                    {
                        if (Session.GetHabbo().VIPRank != 2)
                        {
                            Session.SendWhisper("¡Solo VIP2 pueden conducir estos camiones!", 1);
                            return;
                        }
                    }
                    if (JobInfo.Ranks.ToList().Where(x => x.Value.MaleFigure.Length > 0).Count() > 0)
                    {
                        if (!Session.GetPlay().IsWorking)
                        {
                            if (JobInfo.Name.Contains("Basurero"))
                            {
                                if (Session.GetHabbo().VIPRank < 2)
                                {
                                    Session.SendWhisper("¡Debes tener el uniforme de Basurero para conducir un camión!", 1);
                                    return;
                                }
                            }
                            else
                            {
                                Session.SendWhisper("¡Debes ponterte el uniforme para conducir este vehículo!", 1);
                                return;
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByFurniId(itemfurni);
            if(VO == null || VO.Count <= 0)
            {
                if (corp <= 0)
                {
                    RoleplayManager.Shout(Session, "* Una grúa ha pasado a recoger el vehículo que " + Session.GetHabbo().Username + " intentaba conducir.", 4);
                    RoleplayManager.PickItem(Session, itemfurni);
                    return;
                }
                else
                {
                    // Verificamos que esté en posición de autos corp
                    if (VehicleJobsManager.getVehicleJobIDByPos(Room.Id, Session.GetRoomUser().X, Session.GetRoomUser().Y) > 0)
                    {
                        // Insertamos en diccionario. Si no se puede insertar...
                        RoleplayManager.VehiclesOwnedID++;
                        if (!PlusEnvironment.GetGame().GetVehiclesOwnedManager().NewVehicleOwned(RoleplayManager.VehiclesOwnedID, itemfurni, vehicle.ItemID, 0, Session.GetHabbo().Id, vehicle.Model, vehicle.MaxFuel, 0, 0, false, false, Room.Id, Session.GetRoomUser().X, Session.GetRoomUser().Y, Session.GetRoomUser().Z, string.Empty.ToString().Split(';'), false, out VehiclesOwned nVO))
                        {
                            RoleplayManager.Shout(Session, "* Una grúa ha pasado a recoger el vehículo de trabajo que " + Session.GetHabbo().Username + " intentaba conducir.", 4);
                            RoleplayManager.PickItem(Session, itemfurni);
                            return;
                        }
                        else
                        {
                            // Volvemos a pedir el diccionario
                            VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByFurniId(itemfurni);
                            if (VO == null || VO.Count <= 0)
                            {
                                RoleplayManager.Shout(Session, "* Una grúa ha pasado a recoger el vehículo de trabajo que " + Session.GetHabbo().Username + " intentaba conducir.", 4);
                                RoleplayManager.PickItem(Session, itemfurni);
                                return;
                            }
                        }
                    }
                    else
                    {
                        RoleplayManager.Shout(Session, "* Una grúa ha pasado a recoger el vehículo de trabajo que " + Session.GetHabbo().Username + " intentaba conducir.", 4);
                        RoleplayManager.PickItem(Session, itemfurni);
                        return;
                    }
                }               
            }
            
            #region Extra Conditions & Checks
            #region Grúa Checks (Lotacion <= 0)
            if (VO[0].Location <= 0)
            {
                RoleplayManager.Shout(Session, "* Una grúa ha pasado a recoger el vehículo que " + Session.GetHabbo().Username + " intentaba conducir.", 4);
                RoleplayManager.PickItem(Session, itemfurni);
                return;
            }
            #endregion

            #region Vehicle Fuel/Traba/State Checks
            //Anexar Condiciones para autos de empresas.
            if (VO[0].Fuel <= 0)
            {
                Session.SendWhisper("Este vehículo no tiene combustible.", 1);
                return;
            }
                       
            if (VO[0].Traba)
            {
                if (VO[0].State == 1)
                {
                    Session.SendWhisper("Este vehículo está bloqueado con traba de seguridad. Si es tuyo usa :abrir.", 1);
                    return;
                }
            }
            //state = 0 -> Normal
            //state = 1 -> Bloqueado con traba
            //state = 2 -> No traba y Averiado
            //state = 3 -> Con traba y Averiado
            //state = 4 -> Grua
            if (VO[0].State >= 2 || VO[0].CarLife <= 0)
            {
                Session.SendWhisper("Vehículo averiado. Usa :servicio mecanico para que lo repare.", 1);
                return;
            }
            #endregion
            
            #region CorpCar Respawn
            //if (corp > 0)
            //{
                // Si conduce un auto de trabajo, pero ya había conducido uno NEW => Cualquier auto que conduzca
                // Verificamos si es el mismo.
                // Si es otro, recogemos el anterior (de existir) y spawn.
                if (Session.GetPlay().CarJobId > 0 && Session.GetPlay().CarJobLastItemId != itemfurni)
                {
                    /*VERFICAR SI LO TRAE CONDUCIENDO ALGUIEN PARA EVITAR DUPLICAR*/
                    // Quitar del diccionario
                    /*PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwnedByFurniId(Session.GetPlay().CarJobLastItemId);
                    //Recoge el item
                    RoleplayManager.PickItem(Session, Session.GetPlay().CarJobLastItemId);
                    //Respawneamos nuevo auto
                    RoleplayManager.SetJobCar(Session, Session.GetPlay().CarJobId);*/
                    RoleplayManager.CheckCorpCarp(Session);
                    Session.GetPlay().CamCargId = 0;
                    Session.SendWhisper("Tu vehículo de trabajo anterior ha sido recogido por conducir uno diferente.", 1);
                }

                if(Session.GetPlay().CarJobId <= 0)
                    Session.GetPlay().CarJobId = VehicleJobsManager.getVehicleJobID(Room.Id, corp, Session.GetRoomUser().X, Session.GetRoomUser().Y);
                if (corp > 0)
                    Session.GetPlay().CarJobLastItemId = itemfurni;
                else
                    Session.GetPlay().CarJobLastItemId = 0;
            //}
            #endregion
            #endregion

            #region Online DriveVars
            //Lo conduce
            Session.GetPlay().DrivingCar = true;
            Session.GetPlay().DrivingInCar = false;
            Session.GetPlay().DrivingCarId = VO[0].Id;// Id de VehiclesOwned;
            Session.GetPlay().DrivingCarItem = itemfurni;

            //Combustible System
            Session.GetPlay().CarType = vehicle.CarType;// Define el gasto de combustible
            Session.GetPlay().CarMaxFuel = vehicle.MaxFuel;
            Session.GetPlay().CarFuel = VO[0].Fuel;
            Session.GetPlay().CarTimer = VO[0].Km;
            Session.GetPlay().CarLife = VO[0].CarLife;

            Session.GetPlay().CarEnableId = vehicle.EffectID;//Coloca el enable para conducir
            Session.GetPlay().CarEffectId = vehicle.EffectID;//Guarda el enable del último auto en conducción.
            Session.GetRoomUser().ApplyEffect(vehicle.EffectID);
            Session.GetRoomUser().FastWalking = true;
            #endregion
                 
            RoleplayManager.PickItem(Session, itemfurni);//Recoge el item
            RoleplayManager.Shout(Session, "*Encendió el motor de su vehículo*", 5);
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "open");// WS FUEL

            #region Job Special Checks & Messages
            #region Check My Car & Alarms
            if (Session.GetHabbo().Id == VO[0].OwnerId)
            {
                Session.SendWhisper("Este vehículo es tuyo.", 1);
            }
            else
            {
                if (VO[0].Alarm)
                {
                    string Owner = PlusEnvironment.GetGame().GetClientManager().GetNameById(VO[0].OwnerId);
                    RoomUser Own = Room.GetRoomUserManager().GetRoomUserByHabbo(VO[0].OwnerId);
                    if (Own != null)
                    {
                        Own.GetClient().SendMessage(new RoomNotificationComposer("UK202", 3, "ALARMA: Alguien ha tomado tu " + VO[0].Model + " en " + Room.Name, ""));
                    }
                    RoleplayManager.Shout(Session, "* Se puede escuchar fuertemente la alarma del Vehículo que " + Session.GetHabbo().Username + " conduce.", 4);
                }
            }
            #endregion            

            if (JobInfo != null)
            {
                #region Basurero
                if (JobInfo.Name.Contains("Basurero"))
                {
                    string recolectorName = "Ninguno";

                    if (Session.GetPlay().BasuTeamId <= 0)
                    {
                        Session.SendWhisper("Recuerda que necesitas un compañero que recolecte la basura. ¡Píde a otro trabajador de Basurero que aborde a tu Camión!", 1);
                        Session.GetPlay().IsBasuChofer = true;
                    }
                    else
                        recolectorName = Session.GetPlay().BasuTeamName;

                    PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session,
                            "compose_basurero|" +
                            "showinfo|" +
                            Session.GetHabbo().Username + "|" + // Chofer
                            recolectorName + "|" + // Recolector
                            Session.GetPlay().BasuTrashCount + "/15|" +
                            Session.GetPlay().IsBasuChofer);
                }
                #endregion

                #region Camionero
                if (JobInfo.Name.Contains("Camioneros"))
                {
                    string ChofName = (VO[0].CamOwnId > 0) ? PlusEnvironment.GetGame().GetClientManager().GetNameById(VO[0].CamOwnId) : "Ninguno.";
                    Session.SendWhisper("Chofer Asignado: " + ChofName + " - Cargamento: " + RoleplayManager.getCamCargName(VO[0].CamCargId) + ".", 1);

                    string DestName = "?";
                    Room DestRoom = RoleplayManager.GenerateRoom(VO[0].CamDest);
                    if (DestRoom != null)
                        DestName = DestRoom.Name;

                    if (!ChofName.Equals("Ninguno."))
                    {
                        string action = VO[0].CamState == 2 ? "entregar" : "depositar";
                        string cargaName = VO[0].CamState == 2 ? "Ninguno" : RoleplayManager.getCamCargName(VO[0].CamCargId);
                        PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_camionero|showinfo|" + action + "|" + cargaName + "|" + ChofName + "|" + DestName);
                    }
                }
                #endregion

                #region Police Condition PENDIENTE
                /*
                if (Session.GetPlay().CarCarCorp == JobSearchID4)// ID de Job de Policía
                {
                    if (Session.GetPlay().EscortingWalk)
                    {
                        GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Session.GetPlay().EscortedName);
                        if (TargetClient != null)
                        {
                            // Subimos al convicto de pasajero
                            #region Stand
                            if (TargetClient.GetRoomUser().isSitting)
                            {
                                TargetClient.GetRoomUser().Statusses.Remove("sit");
                                TargetClient.GetRoomUser().Z += 0.35;
                                TargetClient.GetRoomUser().isSitting = false;
                                TargetClient.GetRoomUser().UpdateNeeded = true;
                            }
                            else if (TargetClient.GetRoomUser().isLying)
                            {
                                TargetClient.GetRoomUser().Statusses.Remove("lay");
                                TargetClient.GetRoomUser().Z += 0.35;
                                TargetClient.GetRoomUser().isLying = false;
                                TargetClient.GetRoomUser().UpdateNeeded = true;
                            }
                            #endregion

                            #region Subir
                            if (Session.GetPlay().PasajerosCount >= Session.GetPlay().CarMaxDoors)
                            {
                                TargetClient.SendWhisper("No hay espacio suficiente para subir de pasajero en ese vehículo.", 1);
                                return;
                            }

                            // Convicto
                            TargetClient.GetPlay().Pasajero = true;
                            TargetClient.GetPlay().ChoferName = Session.GetHabbo().Username;
                            TargetClient.GetPlay().ChoferID = Session.GetHabbo().Id;
                            TargetClient.GetRoomUser().CanWalk = false;
                            TargetClient.GetRoomUser().FastWalking = true;
                            TargetClient.GetRoomUser().TeleportEnabled = true;
                            TargetClient.GetRoomUser().AllowOverride = true;

                            // Policia
                            Session.GetPlay().Chofer = true;
                            Session.GetPlay().Pasajeros += TargetClient.GetHabbo().Username + ";";
                            Session.GetPlay().PasajerosCount++;
                            Session.GetRoomUser().FastWalking = true;
                            Session.GetRoomUser().AllowOverride = true;

                            //Animación de subir al Auto
                            int NewX = Session.GetRoomUser().X;
                            int NewY = Session.GetRoomUser().Y;
                            Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(TargetClient.GetRoomUser(), new Point(NewX, NewY), 0, Room.GetGameMap().SqAbsoluteHeight(NewX, NewY)));
                            TargetClient.GetRoomUser().MoveTo(NewX, NewY);
                            #endregion

                            RoleplayManager.Shout(Session, "*Sube a " + TargetClient.GetHabbo().Username + " a la Patrulla*", 5);
                        }
                    }
                }
                */
                #endregion
            }
            #endregion

            Session.GetPlay().CooldownManager.CreateCooldown("car", 1000, 3);            
            return;
            #endregion
        }

    }
}
