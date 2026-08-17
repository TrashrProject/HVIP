using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.Core;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboRoleplay.Vehicles;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Checks the users roleplay conditions
    /// </summary>
    public class ConditionCheckTimer : RoleplayTimer
    {
        public ConditionCheckTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params)
            : base(Type, Client, Time, Forever, Params)
        {
            TimeCount = 0;
        }

        /// <summary>
        /// Checks the users roleplay conditions
        /// </summary>
        public override void Execute()
        {
            try
            {
                #region Base Conditions
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null)
                {
                    base.EndTimer();
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetHabbo().CurrentRoom == null)
                    return;
                #endregion

                #region Variables
                bool Equipped = base.Client.GetPlay().EquippedWeapon == null ? false : true;
                bool Hygiene = base.Client.GetPlay().Hygiene == 0 ? true : false;
                bool Healing = base.Client.GetPlay().BeingHealed;
                bool Farming = base.Client.GetPlay().WateringCan;
                int Effect = base.Client.GetRoomUser().CurrentEffect;
                int Item = base.Client.GetRoomUser().CarryItemID;
                #endregion

                #region Staff Login Manager
                if (base.Client.GetHabbo().Rank > 2 && !base.Client.GetRoomUser().IsAsleep)
                    base.Client.GetPlay().StaffCounterTime++;
                #endregion

                #region Random Checks
                if ((base.Client != null && base.Client.GetRoomUser() != null) && (base.Client.GetPlay().IsWorking || base.Client.GetPlay().IsWorkingOut || !base.Client.GetRoomUser().IsAsleep))
                {
                    if (!base.Client.GetPlay().CaptchaSent)
                        base.Client.GetPlay().CaptchaTime++;
                }

                if (base.Client.GetRoomUser().IsAsleep)
                {
                    if (Equipped)
                    {
                        base.Client.GetPlay().EquippedWeapon = null;
                        if (base.Client.GetRoomUser().CurrentEffect != EffectsList.None)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        if (base.Client.GetRoomUser().CarryItemID != 0)
                            base.Client.GetRoomUser().CarryItem(0);
                    }
                }

                if (!Equipped)
                {
                    if (Effect > 0 && WeaponManager.Weapons.Values.Where(x => x.EffectID == Effect).ToList().Count > 0)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                    if (Item > 0 && WeaponManager.Weapons.Values.Where(x => x.HandItem == Item).ToList().Count > 0)
                        base.Client.GetRoomUser().CarryItem(EffectsList.None);
                }

                //Al salir a la Calle, recolocar el auto si entró antes sin estacionar.
                if (base.Client.GetHabbo().CurrentRoom.DriveEnabled && !base.Client.GetPlay().DrivingCar)
                {
                    if (base.Client.GetPlay().DrivingInCar)
                    {
                        RoleplayManager.Shout(base.Client, "*Sale del establecimiento y arranca su vehículo*", 5);
                        base.Client.GetPlay().DrivingInCar = false;
                        base.Client.GetPlay().DrivingCar = true;
                        base.Client.GetPlay().CarEnableId = base.Client.GetPlay().CarEffectId;
                        // Abrimos ventana de combustible
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_vehicle", "open");
                    }
                }
                #endregion

                #region Anti-Enable Checks
                if (Effect == EffectsList.Invisible && !base.Client.GetPlay().Pasajero)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Repairing Fence & Event Capturing Check
                if (Effect == EffectsList.SunnyD && (!base.Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("repair") || !base.Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("capture")))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Working Out
                if ((Effect == EffectsList.Treadmill || Effect == EffectsList.CrossTrainer) && !base.Client.GetPlay().IsWorkingOut)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Farming Check
                if (Effect == EffectsList.WateringCan && !Farming)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Hygiene Check
                if (Effect == EffectsList.Flies && !Hygiene)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Stun & Frozen Check
                if ((Effect == EffectsList.Dizzy) && !base.Client.GetRoomUser().Frozen)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Riding Horse Check
                if (Effect == EffectsList.HorseRiding && !base.Client.GetRoomUser().RidingHorse)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Healing check
                if (Effect == EffectsList.GreenGlow && !Healing)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // GodMode Effect
                if(Effect == EffectsList.Fireflies && !base.Client.GetPlay().GodMode)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                // Staff On Duty Check
                /*
                if (Effect == EffectsList.Staff && !base.Client.GetPlay().StaffOnDuty)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Ambassador On Duty Check
                if (Effect == EffectsList.Ambassador && !base.Client.GetPlay().AmbassadorOnDuty)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                */
                // Police On Duty Check
                if (Effect == EffectsList.HoloRPPolice && (!base.Client.GetPlay().IsWorking || !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "guide")))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Hosptial On Duty Check
                if (Effect == EffectsList.Medic && (!base.Client.GetPlay().IsWorking || !PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "heal")))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Car Driving Check
                if ((Effect == EffectsList.CarStaff || Effect == EffectsList.CarAmbulance || Effect == EffectsList.CarJuice || Effect == EffectsList.CarBunny || Effect == EffectsList.CarDog || Effect == EffectsList.CarPolice || Effect == EffectsList.CarDollar || Effect == EffectsList.CarTopFuel || Effect == EffectsList.CarMini || Effect == EffectsList.HoverboardYellow) && !base.Client.GetPlay().DrivingCar)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Cuffed Check
                if (Effect == EffectsList.Cuffed && !base.Client.GetPlay().Cuffed)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Taxi Check (Regular Users)
                if (Effect == EffectsList.Taxi && !base.Client.GetPlay().InsideTaxi && !base.Client.GetPlay().CallingTaxi)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Taxi Check (Police)
                if (Effect == EffectsList.PoliceTaxi && !base.Client.GetPlay().InsideTaxi)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Habnam Dance
                if (Effect == EffectsList.GoldDance && (!base.Client.GetPlay().IsFarming || base.Client.GetPlay().WateringCan))
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // StunGun
                if (Effect == EffectsList.StunGun && !base.Client.GetPlay().IsWorking)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // TaxiBot Check
                if((Effect == EffectsList.TaxiPasajero || Effect == EffectsList.TaxiChofer) && base.Client.GetHabbo().TaxiChofer <= 0)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                // Passive Check
                if (Effect == EffectsList.Passive && !base.Client.GetPlay().PassiveMode)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                #endregion

                #region Main Checks
                // Si está siendo Escoltado
                if (base.Client.GetHabbo().EscortID > 0)
                {
                    if (base.Client.GetRoomUser().CanWalk)
                        base.Client.GetRoomUser().CanWalk = false;
                }

                // Si está siendo transportado en Taxi
                if (base.Client.GetHabbo().TaxiChofer > 0)
                {
                    if (base.Client.GetRoomUser().CanWalk)
                        base.Client.GetRoomUser().CanWalk = false;

                    if (!base.Client.GetRoomUser().FastWalking)
                        base.Client.GetRoomUser().FastWalking = true;
                }

                //Simultaneo
                if (base.Client.GetPlay().IsEscorted)
                {
                    if (base.Client.GetRoomUser().CanWalk)
                        base.Client.GetRoomUser().CanWalk = false;                    
                    if (!base.Client.GetRoomUser().TeleportEnabled)
                        base.Client.GetRoomUser().TeleportEnabled = true;
                    if (!base.Client.GetRoomUser().AllowOverride)
                        base.Client.GetRoomUser().AllowOverride = true;
                }
                //end simultaneo

                // Si es pasajero quitamos el god mode
                if(base.Client.GetPlay().Pasajero && base.Client.GetPlay().GodMode)
                {
                    base.Client.GetPlay().GodMode = false;
                    base.Client.GetPlay().GodModeTicks = 0;
                    base.Client.GetPlay().FirstTickBool = false;
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                }

                if (base.Client.GetPlay().DrivingCar)
                {
                    // Si God == true detenemos inmunidad 
                    /* agregar verificacion de que tipo de carro maneja para darle el beneficio de GodMode */
                    if (base.Client.GetPlay().GodMode)
                    {
                        base.Client.GetPlay().GodMode = false;
                        base.Client.GetPlay().GodModeTicks = 0;
                        base.Client.GetPlay().FirstTickBool = false;
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                    }
                    // No consume combustible mientras esté llenando
                    if (!base.Client.GetPlay().IsFuelCharging)
                        base.Client.GetPlay().CarTimer++;

                    if (!base.Client.GetRoomUser().FastWalking)
                        base.Client.GetRoomUser().FastWalking = true;

                    if (base.Client.GetPlay().Chofer && !base.Client.GetRoomUser().AllowOverride)
                        base.Client.GetRoomUser().AllowOverride = true;

                    if (base.Client.GetPlay().SexTimer > 0)
                    {
                        base.Client.GetPlay().SexTimer = 0;
                        base.Client.GetHabbo().Poof(true);
                    }

                    if (base.Client.GetHabbo().CurrentRoom == null)
                    {
                        base.Client.GetPlay().DrivingCar = false;
                        base.Client.GetPlay().CarEnableId = 0;
                    }
                    else
                    {
                        if (base.Client.GetPlay().CarEnableId == EffectsList.CarPolice)
                        {
                            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "arrest") || !base.Client.GetPlay().IsWorking)
                            {
                                base.Client.GetPlay().DrivingCar = false;
                                base.Client.GetPlay().CarEnableId = EffectsList.None;
                                RoleplayManager.Shout(base.Client, "*La patrulla de " + base.Client.GetHabbo().Username + " ha sido devuelta a la comisaría*", 4);
                                base.Client.GetPlay().Ficha = 0;
                                base.Client.GetPlay().FichaTimer = 0;
                            }
                        }
                        //Si Conduce en un lugar donde no es DRIVING
                        if (!base.Client.GetHabbo().CurrentRoom.DriveEnabled && base.Client.GetPlay().DrivingCar)
                        {
                            bool VipCar = base.Client.GetPlay().CarEnableId == EffectsList.HoverBoardWhite;

                            base.Client.GetPlay().DrivingInCar = true;//"Guardar auto para colocarlo al salir"
                            base.Client.GetPlay().DrivingCar = false;
                            base.Client.GetPlay().CarEnableId = EffectsList.None;
                            RoleplayManager.Shout(base.Client, "*Estaciona su vehículo afuera y entra al lugar*", 5);
                            if (base.Client.GetPlay().Ficha > 0)
                            {
                                base.Client.GetPlay().Ficha = 0;
                                base.Client.GetPlay().FichaTimer = 0;
                                RoleplayManager.Shout(base.Client, "*Apaga el Taxímetro y deja de trabajar*", 5);
                            }
                            base.Client.GetRoomUser().FastWalking = false;

                            //Si lleva pasajeros
                            #region Pasajeros
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

                                        // Por seguridad: TP => Chofer
                                        Room Room = RoleplayManager.GenerateRoom(base.Client.GetRoomUser().RoomId);
                                        int NewX = PJ.GetRoomUser().X;
                                        int NewY = PJ.GetRoomUser().Y;
                                        Room.SendMessage(Room.GetRoomItemHandler().UpdateUserOnRoller(base.Client.GetRoomUser(), new Point(NewX, NewY), 0, Room.GetGameMap().SqAbsoluteHeight(NewX, NewY)));
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

                            // Cerramos ventana de combustible
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_vehicle", "close");

                        }
                    }

                    if (base.Client.GetPlay().DrivingCar && base.Client.GetPlay().CarEnableId != EffectsList.HoverBoardWhite && !base.Client.GetPlay().IsFuelCharging)
                    {
                        int MaxGas = 0;

                        #region Vehicle Check
                        Vehicle vehicle = null;
                        int corp = 0;
                        bool ToDB = true;
                        foreach (Vehicle Vehicle in VehicleManager.Vehicles.Values)
                        {
                            if (base.Client.GetPlay().CarEnableId == Convert.ToInt32(Vehicle.EffectID))
                            {
                                vehicle = Vehicle;
                                MaxGas = vehicle.MaxFuel;
                                if (vehicle.CarCorp > 0)
                                {
                                    corp = vehicle.CarCorp;
                                    ToDB = false;
                                }
                            }
                        }
                        #endregion

                        // 60 segs. (30 mins aprox con vehículo encendido)
                        if (base.Client.GetPlay().CarTimer >= 60)
                        {
                            if (base.Client.GetPlay().CarType == 3)
                            {
                                base.Client.GetPlay().CarFuel -= 3;
                                if (base.Client.GetPlay().CarEnableId != EffectsList.CarPolice)
                                    base.Client.GetPlay().CarLife -= 2;
                                //base.Client.SendWhisper("-3L de Combustible - [" + base.Client.GetPlay().CarFuel + " / " + MaxGas + "L]", 1);
                            }
                            else if (base.Client.GetPlay().CarType == 2)
                            {
                                base.Client.GetPlay().CarFuel -= 2;
                                if (base.Client.GetPlay().CarEnableId != EffectsList.CarPolice)
                                    base.Client.GetPlay().CarLife -= 1;
                                //base.Client.SendWhisper("-2L de Combustible - [" + base.Client.GetPlay().CarFuel + " / " + MaxGas + "L]", 1);
                            }
                            else
                            {
                                base.Client.GetPlay().CarFuel -= 1;
                                if (base.Client.GetPlay().CarEnableId != EffectsList.CarPolice)
                                    base.Client.GetPlay().CarLife -= 3;
                                //base.Client.SendWhisper("-1L de Combustible - [" + base.Client.GetPlay().CarFuel + " / " + MaxGas + "L]", 1);
                            }
                            base.Client.GetPlay().CarTimer = 0;
                            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_vehicle", "open");
                        }

                        if (base.Client.GetPlay().CarFuel <= 0 || base.Client.GetPlay().CarLife <= 0)
                        {
                            if (base.Client.GetPlay().CarFuel < 0)
                                base.Client.GetPlay().CarFuel = 0;

                            if (base.Client.GetPlay().CarLife < 0)
                                base.Client.GetPlay().CarLife = 0;

                            if (base.Client.GetPlay().CarLife <= 0)
                            {
                                #region Set Vehicle State
                                List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(base.Client.GetPlay().DrivingCarId);
                                if (VO != null && VO.Count > 0)
                                {
                                    if (VO[0].State == 0 || VO[0].State == 1)
                                    {
                                        if (VO[0].State == 0)
                                            VO[0].State = 2;// averiado y sin traba
                                        else
                                            VO[0].State = 3;// averiado y con traba
                                    }

                                    #region Messages
                                    if (VO[0].Fuel <= 0)
                                    {
                                        RoleplayManager.Shout(base.Client, "* se puede apreciar cómo el vehículo de " + base.Client.GetHabbo().Username + " se apaga debido a que se quedó sin combustible.", 4);
                                        base.Client.SendWhisper("¡Tu vehículo se ha quedado sin combustible! Puedes :usarbidon para recargarlo. Ve a comprarlos a una Gasolinera.", 1);
                                    }
                                    if (VO[0].State == 2 || VO[0].State == 3)
                                    {
                                        RoleplayManager.Shout(base.Client, "* se puede escuchar cómo el motor del vehículo de " + base.Client.GetHabbo().Username + " cruje avieriandose.", 4);
                                        base.Client.SendWhisper("¡Tu vehículo se ha averiado! Puedes usar el ':servicio mecanico' para llamar a un Mecánico en servicio y lo repare.", 1);
                                    }
                                    #endregion
                                }
                                #endregion
                            }

                            #region Park
                            int ItemPlaceId = 0;
                            int roomid = base.Client.GetRoomUser().RoomId;
                            Room Room = RoleplayManager.GenerateRoom(roomid, false);
                            VehiclesOwned VOD = null;
                            if (base.Client.GetPlay().DrivingInCar || !RoleplayManager.isValidPark(Room, base.Client.GetRoomUser().Coordinate))
                            {
                                if (base.Client.GetPlay().CarFuel <= 0)
                                    RoleplayManager.Shout(base.Client, "* Una Grúa se ha llevado el vehículo que " + base.Client.GetHabbo().Username + " conducía por encontrarse mal estacionado y sin combustible.", 4);
                                else
                                    RoleplayManager.Shout(base.Client, "* Una Grúa se ha llevado el vehículo que " + base.Client.GetHabbo().Username + " conducía por encontrarse mal estacionado y averiado", 4);
                                // Actualizamos datos del auto en el diccionario y DB
                                PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(base.Client, 0, ToDB, out VOD);
                                ItemPlaceId = VOD.Id;
                                if (corp > 0)
                                {
                                    PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(base.Client.GetPlay().DrivingCarId);
                                    RoleplayManager.CheckCorpCarp(base.Client);
                                }
                            }
                            else if (base.Client.GetPlay().DrivingCar)
                            {
                                /* OFF - Autos de trabajos sí pueden quedarse sin combustible y averiarse.
                                if (corp > 0)
                                {
                                    if (base.Client.GetPlay().CarFuel <= 0)
                                        RoleplayManager.Shout(Client, "* Una Grúa se ha llevado el vehículo que " + Client.GetHabbo().Username + " conducía por encontrarse mal estacionado y sin combustible.", 4);
                                    else
                                        RoleplayManager.Shout(Client, "* Una Grúa se ha llevado el vehículo que " + Client.GetHabbo().Username + " conducía por encontrarse mal estacionado y averiado.", 4);

                                    PlusEnvironment.GetGame().GetVehiclesOwnedManager().DeleteVehicleOwned(Client.GetPlay().DrivingCarId);
                                    RoleplayManager.CheckCorpCarp(Client);
                                }
                                else
                                {*/
                                // Colocamos Furni en Sala
                                base.Client.GetPlay().isParking = true;
                                Item ItemPlace = RoleplayManager.PutItemToRoom(base.Client, base.Client.GetPlay().DrivingCarItem, base.Client.GetRoomUser().RoomId, vehicle.ItemID, base.Client.GetRoomUser().X, base.Client.GetRoomUser().Y, base.Client.GetRoomUser().RotBody, ToDB);
                                //Item ItemPlace = RoleplayManager.PlaceItemToRoom(base.Client, vehicle.ItemID, 0, base.Client.GetRoomUser().X, base.Client.GetRoomUser().Y, base.Client.GetRoomUser().Z, base.Client.GetRoomUser().RotBody, false, Room.Id, ToDB, "");
                                base.Client.GetPlay().isParking = false;
                                ItemPlaceId = ItemPlace.Id;
                                // Actualizamos datos del auto en el diccionario y DB
                                PlusEnvironment.GetGame().GetVehiclesOwnedManager().UpdateVehicleOwner(base.Client, ItemPlaceId, ToDB, out VOD);
                                //}
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
                                    if (PJ.GetRoomUser() != null)
                                    {
                                        PJ.GetRoomUser().CanWalk = true;
                                        PJ.GetRoomUser().FastWalking = false;
                                        PJ.GetRoomUser().TeleportEnabled = false;
                                        PJ.GetRoomUser().AllowOverride = false;
                                    }

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
                    }

                    /*
                    if (base.Client.GetPlay().DrivingCar && (base.Client.GetPlay().CarEnableId == EffectsList.HoverBoardWhite))
                    {
                        if (base.Client.GetPlay().CarTimer >= 360)
                        {
                            base.Client.GetPlay().CarTimer = 0;
                            base.Client.GetPlay().DrivingCar = false;

                            base.Client.GetPlay().Ficha = 0;
                            base.Client.GetPlay().FichaTimer = 0;

                            if (Effect != EffectsList.None)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                            if (base.Client.GetPlay().CarEnableId == EffectsList.HoverBoardWhite)
                                RoleplayManager.Shout(base.Client, "*Feels their VIP hoverboard stop short, the battery must've died*", 4);

                            base.Client.GetPlay().CarEnableId = EffectsList.None;
                        }
                    }
                    */

                    if (base.Client.GetPlay().DrivingCar && Effect != base.Client.GetPlay().CarEnableId)
                        base.Client.GetRoomUser().ApplyEffect(base.Client.GetPlay().CarEnableId);

                    #region If Taxista by Jeihden OLD OFF
                    /*
                    if (base.Client.GetPlay().Ficha > 0)
                    {//Si ficha > 0 por lógica es Taxista & Está trabajando. Evitamos Ifs innecesarios.

                        //Obetenmos Pasajero(s)
                        //Vars
                        string Pasajeros = base.Client.GetPlay().Pasajeros;
                        Console.WriteLine("Pasajeros: " + Pasajeros);
                        if (Pasajeros != null && Pasajeros != "")
                        {
                            //Si tiene pasajero el Taxímetro corre, si no, nunca avanza.
                            if (base.Client.GetPlay().PasajerosCount > 0)
                            {
                                base.Client.GetPlay().FichaTimer++;

                                if (base.Client.GetPlay().FichaTimer >= 15)//15 segundos
                                {
                                    string[] stringSeparators = new string[] { ";" };
                                    string[] result;
                                    result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                    foreach (string s in result)
                                    {
                                        base.Client.GetPlay().FichaTimer = 0;
                                        Console.WriteLine("foreach: " + s);
                                        GameClient Pasajero = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(s);
                                        if (Pasajero != null)
                                        {
                                            if (Pasajero.GetPlay().Pasajero && Pasajero.GetPlay().ChoferName == base.Client.GetHabbo().Username)
                                            {
                                                if (Pasajero.GetHabbo().Credits < base.Client.GetPlay().Ficha)
                                                {
                                                    base.Client.SendWhisper("El pasajero " + Pasajero.GetHabbo().Username + " no tiene dinero suficiente para pagar la Tarifa. ¡Tú decides si llevarlo o no!", 1);
                                                    base.Client.SendWhisper("Usa ':bajar [nombre-pasajero]' para bajar a alguien de tu vehículo.", 1);
                                                }
                                                else
                                                {
                                                    //Chofer
                                                    base.Client.GetHabbo().Credits += base.Client.GetPlay().Ficha;
                                                    base.Client.GetHabbo().UpdateCreditsBalance();
                                                    base.Client.SendWhisper("Recibes $" + base.Client.GetPlay().Ficha + " de " + Pasajero.GetHabbo().Username + " por la Tarifa tu Taxi.", 1);
                                                    //Pasajero(s)
                                                    Pasajero.GetHabbo().Credits -= base.Client.GetPlay().Ficha;
                                                    Pasajero.GetHabbo().UpdateCreditsBalance();
                                                    Pasajero.SendWhisper("Pagas [-$" + base.Client.GetPlay().Ficha + "] por la Tarifa del Taxi.", 1);
                                                }
                                            }
                                        }
                                        base.Client.GetPlay().FichaTimer = 15;
                                    }
                                }
                            }
                            else//Reseteamos el FichaTimer
                            {
                                if (base.Client.GetPlay().FichaTimer != 0)
                                    base.Client.GetPlay().FichaTimer = 0;
                            }
                        }
                        else//Reseteamos el FichaTimer
                        {
                            if (base.Client.GetPlay().FichaTimer != 0)
                                base.Client.GetPlay().FichaTimer = 0;
                        }
                    }
                    */
                    #endregion

                    #region Check Taximeter
                    if (base.Client.GetPlay().Ficha > 0)
                    {//Si ficha > 0 por lógica es Taxista & Está trabajando. Evitamos Ifs innecesarios.
                        //Obetenmos Pasajero(s)
                        //Vars
                        string Pasajeros = base.Client.GetPlay().Pasajeros;
                        if (Pasajeros != null && Pasajeros != "")
                        {
                            //Si tiene pasajero el Taxímetro corre, si no, nunca avanza.
                            if (base.Client.GetPlay().PasajerosCount > 0)
                            {
                                string[] stringSeparators = new string[] { ";" };
                                string[] result;
                                result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                                foreach (string s in result)
                                {
                                    GameClient Pasajero = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(s);
                                    if (Pasajero != null)
                                    {
                                        if (Pasajero.GetPlay().Pasajero && Pasajero.GetPlay().ChoferName == base.Client.GetHabbo().Username)
                                        {
                                            // Creamos timer si no lo tiene
                                            if (!Pasajero.GetPlay().TimerManager.ActiveTimers.ContainsKey("taxi"))
                                            {
                                                Pasajero.GetPlay().TaxiTimeLeft = RoleplayManager.TaxiTime;
                                                Pasajero.GetPlay().TimerManager.CreateTimer("taxi", 1000, true);
                                            }
                                        }
                                        else
                                        {
                                            // Quitamos timer si lo tiene
                                            if (Pasajero.GetPlay().TimerManager.ActiveTimers.ContainsKey("taxi"))
                                            {
                                                Pasajero.GetPlay().TaxiTimeLeft = 0;
                                                RoleplayTimer Junk;
                                                Pasajero.GetPlay().TimerManager.ActiveTimers.TryRemove("taxi", out Junk);
                                                Console.WriteLine("Quitamos timer taxi para " + Pasajero.GetHabbo().Username);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    if (!base.Client.GetPlay().DrivingCar)
                    {
                        base.Client.GetPlay().CarTimer = 0;

                        if (Effect != EffectsList.None)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                        if (base.Client.GetPlay().Ficha != 0)
                            base.Client.GetPlay().Ficha = 0;

                        if (base.Client.GetPlay().FichaTimer != 0)
                            base.Client.GetPlay().FichaTimer = 0;

                        if (base.Client.GetRoomUser().FastWalking)
                            base.Client.GetRoomUser().FastWalking = false;

                        if (base.Client.GetRoomUser().AllowOverride)
                            base.Client.GetRoomUser().AllowOverride = false;

                        if (base.Client.GetPlay().CooldownManager.ActiveCooldowns.ContainsKey("car"))
                            base.Client.GetPlay().CooldownManager.ActiveCooldowns["car"].Amount = 90;
                        else
                            base.Client.GetPlay().CooldownManager.CreateCooldown("car", 1000, 90);
                    }
                }
                else if (base.Client.GetPlay().Pasajero)
                {
                    if (base.Client.GetRoomUser() != null)
                    {
                        if (base.Client.GetRoomUser().CanWalk)
                            base.Client.GetRoomUser().CanWalk = false;
                        if (!base.Client.GetRoomUser().FastWalking)
                            base.Client.GetRoomUser().FastWalking = true;
                        if (!base.Client.GetRoomUser().TeleportEnabled)
                            base.Client.GetRoomUser().TeleportEnabled = true;
                        if (!base.Client.GetRoomUser().AllowOverride)
                            base.Client.GetRoomUser().AllowOverride = true;
                    }
                }
                else if (base.Client.GetHabbo().TaxiChofer > 0 && Effect != EffectsList.TaxiPasajero)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.TaxiPasajero);
                else if (base.Client.GetPlay().PassiveMode && Effect != EffectsList.Passive && base.Client.GetHabbo().TaxiChofer <= 0 && !base.Client.GetPlay().CallingTaxi)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.Passive);
                else if (base.Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("repair") || base.Client.GetPlay().TimerManager.ActiveTimers.ContainsKey("capture"))
                {
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.SunnyD);
                    return;
                }
                else if (Healing)
                {
                    if (Equipped && Item != base.Client.GetPlay().EquippedWeapon.HandItem)
                        base.Client.GetRoomUser().CarryItem(0);
                    if (Effect != EffectsList.GreenGlow)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.GreenGlow);
                    return;
                }
                else if (base.Client.GetPlay().IsWorkingOut)
                    return;
                else if (base.Client.GetPlay().TextTimer > 0 || base.Client.GetPlay().UsingPhone)
                {
                    if (Effect == EffectsList.CellPhone)
                    {
                        if (base.Client.GetPlay().TextTimer == 1)
                        {
                            base.Client.GetPlay().TextTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetPlay().TextTimer--;
                    }
                    else
                        base.Client.GetPlay().TextTimer = 0;
                }
                else if (base.Client.GetPlay().SexTimer > 0)
                {
                    if (Effect == EffectsList.RunningMan)
                    {
                        if (base.Client.GetPlay().SexTimer == 1)
                        {
                            base.Client.GetPlay().SexTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);

                            base.Client.GetHabbo().Poof(true);
                        }
                        else
                            base.Client.GetPlay().SexTimer--;
                    }
                    else
                    {
                        base.Client.GetPlay().SexTimer = 0;
                        base.Client.GetHabbo().Poof(true);
                    }
                }
                else if (base.Client.GetPlay().RapeTimer > 0)
                {
                    if (Effect == EffectsList.Twinkle)
                    {
                        if (base.Client.GetPlay().RapeTimer == 1)
                        {
                            base.Client.GetPlay().RapeTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetPlay().RapeTimer--;
                    }
                    else if (Effect == EffectsList.Penis)
                    {
                        if (base.Client.GetPlay().RapeTimer == 1)
                        {
                            base.Client.GetPlay().RapeTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetPlay().RapeTimer--;
                    }
                    else
                        base.Client.GetPlay().RapeTimer = 0;
                }
                else if (base.Client.GetPlay().KissTimer > 0)
                {
                    if (Effect == EffectsList.Love)
                    {
                        if (base.Client.GetPlay().KissTimer == 1)
                        {
                            base.Client.GetPlay().KissTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetPlay().KissTimer--;
                    }
                    else
                        base.Client.GetPlay().KissTimer = 0;
                }
                else if (base.Client.GetPlay().HugTimer > 0)
                {
                    if (Effect == EffectsList.Love)
                    {
                        if (base.Client.GetPlay().HugTimer == 1)
                        {
                            base.Client.GetPlay().HugTimer = 0;

                            if (base.Client.GetRoomUser() != null)
                                base.Client.GetRoomUser().ApplyEffect(EffectsList.None);
                        }
                        else
                            base.Client.GetPlay().HugTimer--;
                    }
                    else
                        base.Client.GetPlay().HugTimer = 0;
                }
                else if (Effect == EffectsList.HorseRiding && base.Client.GetRoomUser().RidingHorse)
                    return;
                else if ((Effect == EffectsList.Dizzy) && base.Client.GetRoomUser().Frozen)
                    return;
                else if (base.Client.GetPlay().InsideTaxi)
                {
                    if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "guide") && base.Client.GetPlay().IsWorking)
                    {
                        if (Effect != EffectsList.PoliceTaxi)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.PoliceTaxi);
                    }
                    else
                    {
                        if (Effect != EffectsList.Taxi)
                            base.Client.GetRoomUser().ApplyEffect(EffectsList.Taxi);
                    }
                    return;
                }
                else if (base.Client.GetPlay().Cuffed)
                {
                    if (Effect != EffectsList.Cuffed)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Cuffed);
                    return;
                }
                else if (Equipped && base.Client.GetHabbo().TaxiChofer <= 0)
                {
                    if (base.Client.GetPlay() != null && base.Client.GetPlay().EquippedWeapon != null && base.Client.GetRoomUser() != null)
                    {
                        if (base.Client.GetPlay().EquippedWeapon.EffectID > 0 && base.Client.GetRoomUser().CurrentEffect != base.Client.GetPlay().EquippedWeapon.EffectID)
                            base.Client.GetRoomUser().ApplyEffect(base.Client.GetPlay().EquippedWeapon.EffectID);
                    }
                    if (base.Client.GetPlay() != null && base.Client.GetPlay().EquippedWeapon != null && base.Client.GetRoomUser() != null)
                    {
                        if (base.Client.GetPlay().EquippedWeapon.HandItem > 0 && base.Client.GetRoomUser().CarryItemID != base.Client.GetPlay().EquippedWeapon.HandItem)
                            base.Client.GetRoomUser().CarryItem(base.Client.GetPlay().EquippedWeapon.HandItem);
                    }
                    return;
                }
                else if (Farming)
                {
                    if (Effect != EffectsList.WateringCan)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.WateringCan);
                    return;
                }
                else if (Hygiene)
                {
                    if (Effect != EffectsList.Flies)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Flies);
                    return;
                }
                /*
                else if (base.Client.GetPlay().StaffOnDuty)
                {
                    if (Effect != EffectsList.Staff)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Staff);
                    return;
                }
                else if (base.Client.GetPlay().AmbassadorOnDuty)
                {
                    if (Effect != EffectsList.Ambassador)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Ambassador);
                    return;
                }
                */
                else if (base.Client.GetPlay().IsWorking && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "law"))
                {
                    if (Effect != EffectsList.StunGun)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.StunGun);
                    return;
                }
                else if (base.Client.GetPlay().IsWorking && PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(base.Client, "heal"))
                {
                    if (Effect != EffectsList.Medic)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Medic);
                    return;
                }
                else if (base.Client.GetPlay().GodMode && !base.Client.GetPlay().DrivingCar && base.Client.GetHabbo().TaxiChofer <= 0)
                {
                    if (Effect != EffectsList.Fireflies)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.Fireflies);
                    return;
                }
                else if (base.Client.GetPlay().IsFarming && !base.Client.GetPlay().WateringCan)
                {
                    if (Effect != EffectsList.GoldDance)
                        base.Client.GetRoomUser().ApplyEffect(EffectsList.GoldDance);
                    return;
                }
                else if (base.Client.GetPlay().CallingTaxi && base.Client.GetPlay().TaxiNodeGo >= 0 && Effect != EffectsList.Taxi)
                    base.Client.GetRoomUser().ApplyEffect(EffectsList.Taxi);
                #endregion
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
            }
        }

    }
}