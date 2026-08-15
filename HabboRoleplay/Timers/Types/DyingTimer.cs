using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.RolePlay.PlayRoom;
using System.Text;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.HabboRoleplay.Vehicles;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class DyingTimer : RoleplayTimer
    {
        public DyingTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds
            OriginalTime = RoleplayManager.DyingTime;
            TimeLeft = Client.GetPlay().DyingTimeLeft * 60000;
        }
 
        /// <summary>
        /// Increases the users hunger
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null || !base.Client.GetPlay().IsDying || base.Client.GetPlay().IsDead)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                if (base.Client.GetPlay().Pasajero)
                {
                    if (base.Client.GetRoomUser().Frozen)
                        base.Client.GetRoomUser().Frozen = false;
                    return;
                }

                if (base.Client.GetPlay().TargetReanim)
                    return;

                if (!base.Client.GetRoomUser().Frozen && !base.Client.GetPlay().Pasajero)
                    base.Client.GetRoomUser().Frozen = true;

                if (base.Client.GetRoomUser().CanWalk)
                    base.Client.GetRoomUser().CanWalk = false;

                if (!base.Client.GetRoomUser().isLying)
                {
                    #region Lays User Down
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
                    }
                    #endregion
                }
                else
                {
                }

                #region Special Checks

                #region Is Driving?
                if (base.Client.GetPlay().DrivingCar)
                {
                    #region Vehicle Check
                    Vehicle vehicle = null;
                    int corp = 0;
                    bool ToDB = true;
                    foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                    {
                        if (base.Client.GetPlay().CarEnableId == Convert.ToInt32(Vehicle.EffectID))
                        {
                            vehicle = Vehicle;
                            if (vehicle.CarCorp > 0)
                            {
                                corp = vehicle.CarCorp;
                                ToDB = false;
                            }
                        }
                    }
                    #endregion

                    #region Park
                    int ItemPlaceId = 0;
                    int roomid = base.Client.GetRoomUser().RoomId;
                    Room RoomDriving = RoleplayManager.GenerateRoom(roomid, false);

                    if (base.Client.GetPlay().DrivingInCar || !RoleplayManager.isValidPark(RoomDriving, base.Client.GetRoomUser().Coordinate))
                    {
                        RoleplayManager.Shout(base.Client, "* Una Grúa se ha llevado el vehículo que " + base.Client.GetHabbo().Username + " conducía por encontrarse mal estacionado.", 4);
                        // Actualizamos datos del auto en el diccionario y DB
                        PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(base.Client, 0, ToDB, out VehiclesOwned VOD);
                        ItemPlaceId = VOD.Id;
                        if (corp > 0)
                        {
                            PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(base.Client.GetPlay().DrivingCarId);
                            RoleplayManager.CheckCorpCarp(base.Client);
                        }
                    }
                    else if (base.Client.GetPlay().DrivingCar)
                    {
                        if (corp > 0)
                        {
                            RoleplayManager.Shout(Client, "* Una Grúa se ha llevado el vehículo que " + Client.GetHabbo().Username + " conducía por encontrarse mal estacionado.", 4);
                            PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                            RoleplayManager.CheckCorpCarp(Client);
                        }
                        else
                        {
                            // Colocamos Furni en Sala
                            base.Client.GetPlay().isParking = true;
                            Item ItemPlace = RoleplayManager.PutItemToRoom(base.Client, base.Client.GetPlay().DrivingCarItem, base.Client.GetRoomUser().RoomId, vehicle.ItemID, base.Client.GetRoomUser().X, base.Client.GetRoomUser().Y, base.Client.GetRoomUser().RotBody, ToDB);
                            //Item ItemPlace = RoleplayManager.PlaceItemToRoom(base.Client, vehicle.ItemID, 0, base.Client.GetRoomUser().X, base.Client.GetRoomUser().Y, base.Client.GetRoomUser().Z, base.Client.GetRoomUser().RotBody, false, RoomDriving.Id, ToDB, "");
                            base.Client.GetPlay().isParking = false;
                            ItemPlaceId = ItemPlace.Id;
                            // Actualizamos datos del auto en el diccionario y DB
                            PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(base.Client, ItemPlaceId, ToDB, out VehiclesOwned VOD);
                        }
                    }

                    #region Extra Conditions & Checks
                    #region CorpCar Respawn
                    if (corp > 0)
                    {
                        base.Client.GetPlay().CarJobLastItemId = ItemPlaceId;
                    }
                    #endregion

                    #region Pasajeros (Algoritmo replicado en ConditionCheckTimer por seguridad)
                    //Vars
                    string Pasajeros = base.Client.GetPlay().Pasajeros;
                    string[] stringSeparators = new string[] { ";" };
                    string[] result;
                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string psjs in result)
                    {
                        GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                        if (PJ != null)
                        {
                            if (PJ.GetPlay().ChoferName == base.Client.GetHabbo().Username)
                            {
                                RoleplayManager.Shout(PJ, "*Baja del vehículo de " + base.Client.GetHabbo().Username + "*", 5);
                            }
                            // PASAJERO
                            PJ.GetPlay().Pasajero = false;
                            PJ.GetPlay().ChoferName = "";
                            PJ.GetPlay().ChoferID = 0;
                            PJ.GetRoomUser().CanWalk = true;
                            PJ.GetRoomUser().FastWalking = false;
                            PJ.GetRoomUser().TeleportEnabled = false;
                            PJ.GetRoomUser().AllowOverride = false;

                            // Descontamos Pasajero
                            base.Client.GetPlay().PasajerosCount--;
                            StringBuilder builder = new StringBuilder(base.Client.GetPlay().Pasajeros);
                            builder.Replace(PJ.GetHabbo().Username + ";", "");
                            base.Client.GetPlay().Pasajeros = builder.ToString();

                            // CHOFER 
                            base.Client.GetPlay().Chofer = (base.Client.GetPlay().PasajerosCount <= 0) ? false : true;
                            base.Client.GetRoomUser().AllowOverride = (base.Client.GetPlay().PasajerosCount <= 0) ? false : true;

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

                                        /*
                                        if (PJ.GetHabbo().CurrentRoomId != ToHosp)
                                            RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                                        else
                                            Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                                        */
                                        RoleplayManager.SendUserTimer(PJ, ToHosp2, "", "death");
                                    }
                                    else
                                    {
                                        PJ.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                        PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                        PJ.GetPlay().RefreshStatDialogue();
                                        PJ.GetRoomUser().Frozen = false;
                                        PJ.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                                    }
                                }
                                else
                                {
                                    PJ.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                                    PJ.GetPlay().CurHealth = PJ.GetPlay().MaxHealth;
                                    PJ.GetPlay().RefreshStatDialogue();
                                    PJ.GetRoomUser().Frozen = false;
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
                    #endregion

                    #region Check Jobs

                    #region Taxista
                    if (base.Client.GetPlay().Ficha != 0)
                    {
                        base.Client.GetPlay().Ficha = 0;
                        base.Client.GetPlay().FichaTimer = 0;
                        RoleplayManager.Shout(base.Client, "*Apaga el Taxímetro y deja de trabajar*", 5);
                    }
                    #endregion

                    #endregion
                    #endregion

                    #region Online ParkVars
                    //Retornamos a valores predeterminados
                    base.Client.GetPlay().DrivingCar = false;
                    base.Client.GetPlay().DrivingInCar = false;
                    base.Client.GetPlay().DrivingCarId = 0;// Id de VehiclesOwned;

                    //Combustible System
                    base.Client.GetPlay().CarType = 0;// Define el gasto de combustible
                    base.Client.GetPlay().CarFuel = 0;
                    base.Client.GetPlay().CarMaxFuel = 0;
                    base.Client.GetPlay().CarTimer = 0;
                    base.Client.GetPlay().CarLife = 0;

                    base.Client.GetPlay().CarEnableId = 0;//Coloca el enable para conducir
                    base.Client.GetPlay().CarEffectId = 0;//Guarda el enable del último auto en conducción.
                    base.Client.GetRoomUser().ApplyEffect(0);
                    base.Client.GetRoomUser().FastWalking = false;
                    #endregion

                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_vehicle", "close");
                    #endregion
                }
                #endregion

                #region Is Pasajero?
                if (base.Client.GetPlay().Pasajero)
                {
                    // PASAJERO
                    base.Client.GetPlay().Pasajero = false;
                    base.Client.GetPlay().ChoferName = "";
                    base.Client.GetPlay().ChoferID = 0;
                    base.Client.GetRoomUser().CanWalk = true;
                    base.Client.GetRoomUser().FastWalking = false;
                    base.Client.GetRoomUser().TeleportEnabled = false;
                    base.Client.GetRoomUser().AllowOverride = false;

                    GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(base.Client.GetPlay().ChoferName);

                    if (TargetClient != null)
                    {
                        // Descontamos Pasajero
                        TargetClient.GetPlay().PasajerosCount--;
                        if (TargetClient.GetPlay().PasajerosCount <= 0)
                            TargetClient.GetPlay().Pasajeros = "";
                        else
                            TargetClient.GetPlay().Pasajeros.Replace(base.Client.GetHabbo().Username + ";", "");

                        // CHOFER 
                        TargetClient.GetPlay().Chofer = (TargetClient.GetPlay().PasajerosCount <= 0) ? false : true;
                        TargetClient.GetRoomUser().AllowOverride = (TargetClient.GetPlay().PasajerosCount <= 0) ? false : true;
                    }
                    RoleplayManager.Shout(base.Client, "*Baja del vehículo*", 5);
                }
                #endregion

                #region Is Working?
                if (base.Client.GetPlay().IsWorking)
                {
                    Client.GetPlay().UpdateTimerDialogue("Work-Timer", "remove", (TimeLeft / 60000), OriginalTime);
                    RoleplayManager.Shout(base.Client, "*Deja de trabajar*", 5);

                    WorkManager.RemoveWorkerFromList(base.Client);
                    base.Client.GetPlay().IsWorking = false;
                    base.Client.GetHabbo().Poof();
                }
                #endregion
                
                #region CheckBasurero
                RoleplayManager.CheckBasurero(Client);
                #endregion

                #region CheckEscort
                RoleplayManager.CheckEscort(Client);
                #endregion

                #region CheckEquippedWeapon
                if (base.Client.GetPlay().EquippedWeapon != null)
                {
                    string UnEquipMessage = base.Client.GetPlay().EquippedWeapon.UnEquipText;
                    UnEquipMessage = UnEquipMessage.Replace("[NAME]", base.Client.GetPlay().EquippedWeapon.PublicName);

                    if (base.Client.GetRoomUser().CurrentEffect == base.Client.GetPlay().EquippedWeapon.EffectID)
                        base.Client.GetRoomUser().ApplyEffect(0);

                    if (base.Client.GetRoomUser().CarryItemID == base.Client.GetPlay().EquippedWeapon.HandItem)
                        base.Client.GetRoomUser().CarryItem(0);

                    base.Client.GetPlay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                    base.Client.GetPlay().EquippedWeapon = null;

                    base.Client.GetPlay().WLife = 0;
                    base.Client.GetPlay().Bullets = 0;
                }
                #endregion

                #endregion

                TimeCount++;
                TimeLeft -= 1000;

                #region IN STATE CONDITIONS by Jeihden
                if (!base.Client.GetPlay().InState)
                {
                    Client.SendWhisper("¡Has muerto! Usa ':servicio medico' para llamar a una ambulancia ó ':aceptarmuerte' para reaparecer en el hospital.", 1);

                    if (Client.GetRoomUser() != null)
                        Client.GetRoomUser().ApplyEffect(0);

                    Client.GetRoomUser().Frozen = true;
                    Client.GetRoomUser().IsWalking = false;
                    #region Lays User Down
                    if (Client.GetRoomUser() != null)
                    {
                        var User = Client.GetRoomUser();

                        if (User.isLying)
                        {
                            User.RemoveStatus("lay");
                            User.isLying = false;
                        }

                        if (User.IsWalking)
                        {
                            User.RemoveStatus("mv");
                            User.IsWalking = false;
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
                    }
                    #endregion

                    base.Client.GetPlay().InState = true;

                    base.Client.GetPlay().Deaths++;
                }
                #endregion

                if (TimeCount == 60)
                    base.Client.GetPlay().DyingTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        //base.Client.SendWhisper("Quedan " + base.Client.GetPlay().DyingTimeLeft + " minuto(s) para mandarse al hospital.", 1);
                        base.Client.SendWhisper("Recuerda que puedes usar ':aceptarmuerte' para reaparecer en el Hospital y esperar el tiempo de Recuperación, o bien, ser atendid@.", 1);
                        TimeCount = 0;
                    }
                    return;
                }

                #region ESTADO MORIR (ó :aceptarmuerte)                
                Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                string MyCity = Room.City;

                PlayRoom Data;
                int ToHosp = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetHospital(MyCity, out Data);

                if (ToHosp > 0)
                {
                    Room Room2 = RoleplayManager.GenerateRoom(ToHosp);
                    if (Room2 != null)
                    {
                        base.Client.GetPlay().IsDead = true;
                        base.Client.GetPlay().DeadTimeLeft = RoleplayManager.DeathTime;

                        base.Client.GetHabbo().HomeRoom = ToHosp;

                        /*
                        if (base.Client.GetHabbo().CurrentRoomId != ToHosp)
                            RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                        else
                            Client.GetPlay().TimerManager.CreateTimer("death", 1000, true);
                        */
                        RoleplayManager.SendUserTimer(Client, ToHosp, "", "death");
                    }
                    else
                    {
                        Client.SendNotification("[Error][102] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                        base.Client.GetPlay().CurHealth = base.Client.GetPlay().MaxHealth;
                        base.Client.GetPlay().RefreshStatDialogue();
                        base.Client.GetRoomUser().Frozen = false;
                        base.Client.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                    }
                }
                else
                {
                    Client.SendNotification("[Error][103] -> Lamentablemente ha habido un error. No se encontró ningún Hospital disponible en esta ciudad. Comunícaselo a un Administrador. ¡Gracias!");
                    base.Client.GetPlay().CurHealth = base.Client.GetPlay().MaxHealth;
                    base.Client.GetPlay().RefreshStatDialogue();
                    base.Client.GetRoomUser().Frozen = false;
                    base.Client.SendWhisper("Se te ha revivido a causa que no hay ningún hospital en esta Ciudad");
                }
                base.Client.GetPlay().IsDying = false;
                base.Client.GetPlay().DyingTimeLeft = 0;
                base.Client.GetPlay().InState = false;
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