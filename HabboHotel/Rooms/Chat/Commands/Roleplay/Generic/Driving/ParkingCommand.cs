using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.VehiclesJobs;
using Plus.HabboRoleplay.VehicleOwned;
using System.Drawing;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class ParkingCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_driving_parking"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Estaciona el vehículo que conduces."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Get Position User Vars
            RoomUser User = Session.GetRoomUser();

            if (User == null)
                return;

            int X = User.X;
            int Y = User.Y;
            double Z = User.Z;
            int Rot = User.RotBody;
            Point Coords = User.Coordinate;
            #endregion

            #region Conditions
            if (Session.GetPlay().TryGetCooldown("park"))
                return;
            if (!Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("¡No te encuentras conduciendo un vehículo!", 1);
                return;
            }
            if (!Room.DriveEnabled)
            {
                Session.SendWhisper("¡No es posible conducir en esta zona!", 1);
                return;
            }
            if (Session.GetRoomUser().isSitting || Session.GetRoomUser().isLying || User.Statusses.ContainsKey("sit") || User.Statusses.ContainsKey("lay"))
            {
                Session.SendWhisper("¡El vehículo no puede ser estacionado en esta posición!", 1);
                return;
            }
            if (Session.GetRoomUser().IsWalking)
            {
                Session.SendWhisper("¡No tan rápido! Debes detener el vehículo completamente.", 1);
                return;
            }
            // Evitar que se estaciones en puntos de control, comodines o flechas
            if (!RoleplayManager.isValidPark(Room, Coords))
            {
                Session.SendWhisper("¡El vehículo no puede ser estacionado en esta posición!", 1);
                return;
            }
            // Evitar que se estacione en spawner de vehiculos de trabajos
            if (VehicleJobsManager.getVehicleJobIDByPos(Room.Id, Session.GetRoomUser().X, Session.GetRoomUser().Y) > 0)
            {
                Session.SendWhisper("¡No está permitido estacionarse en esta posición!", 1);
                return;
            }
            #endregion

            #region Get Information form VehiclesManager
            Vehicle vehicle = null;
            int corp = 0;
            bool ToDB = true;
            foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
            {
                if (Session.GetPlay().CarEffectId == Vehicle.EffectID)
                {
                    vehicle = Vehicle;
                    corp = Convert.ToInt32(Vehicle.CarCorp);
                    if (corp > 0)
                        ToDB = false;
                }
            }
            if (vehicle == null)
            {
                Session.SendWhisper("¡Ha ocurrido un error al buscar los datos del vehículo que conduces!", 1);
                return;
            }
            #endregion

            #region Park
            // Colocamos Furni en Sala
            Session.GetPlay().isParking = true;
            Item Item = RoleplayManager.PutItemToRoom(Session, Session.GetPlay().DrivingCarItem, Session.GetRoomUser().RoomId, vehicle.ItemID, X, Y, Rot, ToDB);
            //Item Item = RoleplayManager.PlaceItemToRoom(Session, vehicle.ItemID, 0, X, Y, Z, Rot, false, Room.Id, ToDB, "");
            Session.GetPlay().isParking = false;

            // Actualizamos datos del auto en el diccionario y DB
            PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(Session, Item.Id, ToDB, out VehiclesOwned VOD);

            #region Extra Conditions & Checks
            #region CorpCar Respawn
            if (corp > 0)
            {
                Session.GetPlay().CarJobLastItemId = Item.Id;
            }
            #endregion

            #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
            //Vars
            string Pasajeros = Session.GetPlay().Pasajeros;
            string[] stringSeparators = new string[] { ";" };
            string[] result;
            result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string psjs in result)
            {
                GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                if (PJ != null && PJ.GetPlay() != null && PJ.GetRoomUser() != null)
                {
                    if (PJ.GetPlay().ChoferName == Session.GetHabbo().Username)
                    {
                        RoleplayManager.Shout(PJ, "*Baja del vehículo de " + Session.GetHabbo().Username + "*", 5);
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
            Session.GetPlay().PasajerosCount = 0;
            Session.GetPlay().Pasajeros = "";
            Session.GetPlay().Chofer = false;
            Session.GetRoomUser().AllowOverride = false;
            #endregion

            #region Check Jobs

            #region Taxista
            if (Session.GetPlay().Ficha != 0)
            {
                Session.GetPlay().Ficha = 0;
                Session.GetPlay().FichaTimer = 0;
                RoleplayManager.Shout(Session, "*Apaga el Taxímetro y deja de trabajar*", 5);
            }
            #endregion

            #region Basurero
            if (Session.GetPlay().BasuTeamId <= 0)
                Session.GetPlay().IsBasuChofer = false;
            #endregion

            #endregion
            #endregion

            #region Online ParkVars
            //Retornamos a valores predeterminados
            Session.GetPlay().DrivingCar = false;
            Session.GetPlay().DrivingInCar = false;
            Session.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

            //Combustible System
            Session.GetPlay().CarType = 0;// Define el gasto de combustible
            Session.GetPlay().CarFuel = 0;
            Session.GetPlay().CarMaxFuel = 0;
            Session.GetPlay().CarTimer = 0;
            Session.GetPlay().CarLife = 0;

            Session.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
            Session.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
            Session.GetRoomUser().ApplyEffect(0);
            Session.GetRoomUser().FastWalking = false;
            #endregion

            RoleplayManager.Shout(Session, "*Detuvo el motor de su vehículo*", 5);
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_vehicle", "close");// WS FUEL
            Session.GetPlay().CooldownManager.CreateCooldown("park", 1000, 3);

            if (corp > 0 && (VOD.CamOwnId == Session.GetHabbo().Id || VOD.CamOwnId == 0))
            {
                int time = RoleplayManager.VehicleJobTime; // 5 mins
                if (vehicle.Model.Contains("Patrulla"))
                    time = RoleplayManager.VehicleJobPoliTime; // 10 mins.

                Session.SendWhisper("Recuerda no abandonar mucho tiempo tu vehículo de trabajo o será decomisado.", 1);
                Session.GetPlay().VehicleTimer = time;
                Session.GetPlay().TimerManager.CreateTimer("vehiclejob", 1000, true);
            }
            return;
            #endregion
        }
    }
}