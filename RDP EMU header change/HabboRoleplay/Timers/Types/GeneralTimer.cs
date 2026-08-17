using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.Core;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.HabboRoleplay.VehicleOwned;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Groups;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Makes the citizen get hungry over time
    /// </summary>
    public class GeneralTimer : RoleplayTimer
    {
        public GeneralTimer(string Type, GameClient Client, int Time, bool Forever, object[] Params) 
            : base(Type, Client, Time, Forever, Params)
        {
            // Convert to milliseconds

            TimeLeft = base.Client.GetPlay().LoadingTimeLeft * 1000;
        }
 
        /// <summary>
        /// Increases the users hunger
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.Client == null || base.Client.GetHabbo() == null || base.Client.GetPlay() == null || base.Client.GetPlay().IsDying || base.Client.GetPlay().IsDead || base.Client.GetPlay().IsJailed || base.Client.GetRoomUser() == null || base.Client.GetRoomUser().IsWalking || base.Client.GetPlay().BreakGeneralTimer)
                {
                    if(!base.Client.GetPlay().CallingTaxi && !base.Client.GetPlay().TogglingPSV)
                        RoleplayManager.Shout(base.Client, "*Ha dejado de realizar la acción en la que estaba*", 5);
                    base.EndTimer();

                    base.Client.GetPlay().BreakGeneralTimer = false;
                    //base.Client.GetPlay().LoadingTimeLeft = 0;

                    #region Retornamos Variables de Timers

                    #region Combustible
                    if (base.Client.GetPlay().IsFuelCharging)
                    {
                        base.Client.GetPlay().IsFuelCharging = false;
                        base.Client.GetPlay().FuelChargingCant = 0;
                        base.Client.SendWhisper("No debes moverte ni apagar tu vehículo hasta que se termine de llenar el tanque.", 1);
                    }
                    #endregion

                    #region Camionero Cargando
                    if (base.Client.GetPlay().IsCamLoading)
                    {
                        base.Client.GetPlay().IsCamLoading = false;
                        base.Client.GetPlay().CamCargId = 0;
                        base.Client.SendWhisper("No debes moverte ni apagar tu Camión hasta que se termine de cargar.", 1);
                    }
                    #endregion

                    #region Camionero Descargando
                    if (base.Client.GetPlay().IsCamUnLoading)
                    {
                        base.Client.GetPlay().IsCamUnLoading = false;
                        base.Client.SendWhisper("No debes moverte ni apagar tu Camión hasta que se termine de descargar.", 1);
                    }
                    #endregion

                    #region Minero
                    if (base.Client.GetPlay().IsMinerLoading)
                        base.Client.GetPlay().IsMinerLoading = false;

                    if (base.Client.GetRoomUser() != null && base.Client.GetRoomUser().CurrentEffect == EffectsList.Pickaxe)
                        base.Client.GetRoomUser().ApplyEffect(0);
                    #endregion

                    #region Mecánico
                    if (base.Client.GetPlay().IsMecLoading)
                        base.Client.GetPlay().IsMecLoading = false;
                    #endregion

                    #region Farming
                    if (base.Client.GetPlay().IsFarming)
                    {
                        base.Client.GetPlay().IsFarming = false;
                        base.Client.GetPlay().WateringCan = false;
                    }
                    #endregion

                    #region Ladrón
                    if (base.Client.GetPlay().IsForcingHouse)
                        base.Client.GetPlay().IsForcingHouse = false;
                    #endregion

                    #region Turfs
                    if (base.Client.GetPlay().TurfCapturing)
                        base.Client.GetPlay().TurfCapturing = false;

                    base.Client.GetPlay().CooldownManager.CreateCooldown("capturing", 1000, 60);

                    if(base.Client.GetRoomUser() != null && base.Client.GetRoomUser().GetRoom() != null && base.Client.GetRoomUser().GetRoom().Group != null)
                    {
                        base.Client.GetRoomUser().GetRoom().Group.GangTurfsDefended++;
                        base.Client.GetRoomUser().GetRoom().Group.UpdateStat("gang_turfs_defend", base.Client.GetRoomUser().GetRoom().Group.GangTurfsDefended);
                    }
                    #endregion

                    #region Call Taxi
                    if(base.Client.GetPlay().CallingTaxi)
                    {
                        base.Client.GetPlay().CallingTaxi = false;
                        base.Client.GetPlay().TaxiNodeGo = -1;

                        if (base.Client.GetRoomUser().CurrentEffect == EffectsList.Taxi)
                            base.Client.GetRoomUser().ApplyEffect(0);
                    }
                    #endregion

                    #region Toggling PSV Mode
                    if (base.Client.GetPlay().TogglingPSV)
                        base.Client.GetPlay().TogglingPSV = false;
                    #endregion

                    #endregion
                    return;
                }

                if (base.Client.GetRoomUser() == null)
                    return;

                if (base.Client.GetRoomUser().IsAsleep)
                    return;

                #region Specific Special Conditions
                if ((base.Client.GetPlay().IsFuelCharging && !base.Client.GetPlay().DrivingCar) || (base.Client.GetPlay().IsCamLoading && !base.Client.GetPlay().DrivingCar) || (base.Client.GetPlay().IsCamUnLoading && !base.Client.GetPlay().DrivingCar))
                {
                    base.Client.GetPlay().BreakGeneralTimer = true;
                    return;
                }
                #endregion

                TimeCount++;
                
                TimeLeft -= 1000;

                base.Client.GetPlay().LoadingTimeLeft--;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 2)
                    {
                        if(!base.Client.GetPlay().TurfCapturing)
                            base.Client.SendWhisper("Debes esperar " + base.Client.GetPlay().LoadingTimeLeft + " segundo(s)...", 1);
                        TimeCount = 0;
                    }
                    if (TimeLeft == 5000)
                    {
                        if (base.Client.GetPlay().IsCamLoading)
                            base.Client.SendWhisper("Los Cargadores están terminando de subir la última carga y cerrando tu compuerta.", 1);
                        if (base.Client.GetPlay().IsCamUnLoading)
                            base.Client.SendWhisper("Los Cargadores están terminando de bajar la última carga y cerrando tu compuerta.", 1);
                        if (base.Client.GetPlay().IsMecLoading)
                            RoleplayManager.Shout(base.Client, "*Hace las últimas pruebas al vehículo comprobando que todo funcione correctamente*", 5);
                        if (base.Client.GetPlay().IsForcingHouse)
                            RoleplayManager.Shout(base.Client, "*Patea fuertemente la puerta de la casa intentando tirarla*", 5);

                    }
                    return;
                }

                #region Cumple el Timer

                #region Fuel Charging
                if (base.Client.GetPlay().IsFuelCharging)
                {
                    int Cant = base.Client.GetPlay().FuelChargingCant;
                    int Price = Cant * RoleplayManager.FuelPrice;
                    base.Client.SendWhisper("¡" + Cant + " L. de Combustible Cargado(s)! Gracias por su compra. (-$" + Price + ")", 1);

                    base.Client.GetHabbo().Credits -= Price;
                    base.Client.GetHabbo().UpdateCreditsBalance();

                    base.Client.GetPlay().IsFuelCharging = false;
                    base.Client.GetPlay().FuelChargingCant = 0;

                    List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(base.Client.GetPlay().DrivingCarId);
                    if (VO != null && VO.Count > 0)
                    {
                        VO[0].Fuel += Cant;
                        RoleplayManager.UpdateVehicleStat(VO[0].Id, "fuel", VO[0].Fuel);// Actualizamos en DB
                        base.Client.GetPlay().CarFuel = VO[0].Fuel;
                    }

                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(base.Client, "event_vehicle", "open");
                    base.Client.GetPlay().LoadingTimeLeft = 0;
                }
                #endregion

                #region Camionero
                if (base.Client.GetPlay().IsCamLoading)
                {
                    List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(base.Client.GetPlay().DrivingCarId);
                    if (VO != null && VO.Count > 0)
                    {
                        Room Room = RoleplayManager.GenerateRoom(VO[0].CamDest);
                        if (Room == null)
                        {
                            base.Client.SendWhisper("Al parecer no se encontró el destino generado para esta ciudad. ((Contacta con un administrador))");

                            base.Client.GetPlay().IsCamLoading = false;
                            base.Client.GetPlay().LoadingTimeLeft = 0;
                            base.EndTimer();
                        }
                        else
                        {
                            VO[0].CamCargId = base.Client.GetPlay().CamCargId;
                            VO[0].CamState = 1; // Cargado
                            VO[0].CamOwnId = base.Client.GetHabbo().Id;
                            base.Client.SendWhisper("¡Camión Cargado! Ahora dirígete a " + Room.Name + " y usa ':depositarcarga'", 1);
                            base.Client.GetPlay().IsCamLoading = false;

                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(base.Client, "compose_camionero|showinfo|depositar|" + RoleplayManager.getCamCargName(VO[0].CamCargId) + "|" + base.Client.GetHabbo().Username + "|" + Room.Name);
                        }
                    }
                }

                if (base.Client.GetPlay().IsCamUnLoading)
                {
                    List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(base.Client.GetPlay().DrivingCarId);
                    if (VO != null && VO.Count > 0)
                    {
                        Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);
                        string MyCity = Room.City;

                        PlayRoom Data;
                        int Camioneros = PlusEnvironment.GetGame().GetPlayRoomManager().TryToGetCamioneros(MyCity, out Data);
                        if (Camioneros < 1)
                        {
                            base.Client.SendWhisper("Al parecer no se encontró la Zona de Camioneros en la Ciudad. ((Contacta con un administrador))");
                            base.Client.GetPlay().IsCamUnLoading = false;
                            base.Client.GetPlay().LoadingTimeLeft = 0;
                            base.EndTimer();
                        }
                        else
                        {
                            base.Client.SendWhisper("¡Carga entregada! Ahora regresa tu camión y usa ':entregarcamion' para recibir tu pago.", 1);
                            VO[0].CamDest = Camioneros;//ID de la base de Camiones 
                            VO[0].CamState = 2; // Desargado
                            base.Client.GetPlay().IsCamUnLoading = false;

                            Room DestRoom = RoleplayManager.GenerateRoom(Camioneros);

                            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(base.Client, "compose_camionero|showinfo|entregar|Ninguno|" + base.Client.GetHabbo().Username + "|" + DestRoom.Name);
                        }
                    }
                }
                #endregion

                #region Ladron
                if (base.Client.GetPlay().IsForcingHouse)
                {
                    bool Force = true;

                    #region Cal Prob
                    if (base.Client.GetPlay().LadronLvl < 3)
                    {
                        Random rnd = new Random();
                        int Prob = rnd.Next(0, 101);

                        if (base.Client.GetPlay().LadronLvl == 1)
                        {
                            // 40% si | 60% no
                            if (Prob >= 60)
                                Force = false;
                        }
                        else if (base.Client.GetPlay().LadronLvl == 2)
                        {
                            // 60% si | 40% no
                            if (Prob <= 40)
                                Force = false;
                        }
                        // 100% si
                    }
                    #endregion
                    var House = base.Client.GetPlay().HouseToForce;
                    
                    if (Force)
                    {
                        
                        if (House != null)
                        {
                            RoleplayManager.Shout(base.Client, "*Abre la puerta de la Casa exitosamente*", 5);
                            base.Client.SendWhisper("¡Tienes unos segundos para ':entrar' y ':robar objeto' antes de que suene la alarma!", 1);
                            RoleplayManager.NoJobSkills(base.Client, "ladron", base.Client.GetPlay().LadronLvl, base.Client.GetPlay().LadronXP);
                        }
                        else
                        {
                            base.Client.SendWhisper("Ha ocurrido un error al obtener la información de la casa. Contacta con un Administrador. [1]", 1);
                        }
                    }
                    else
                    {
                        RoleplayManager.Shout(base.Client, "*No pudo abrir la puerta de la Casa*", 5);
                        RoleplayManager.Shout(base.Client, "* Los Vecinos se han alertado y han llamado a las Autoridades.", 4);
                    }
                    long TimeForcing = DateTimeOffset.Now.ToUnixTimeSeconds();

                    // Si no pudo abrir, entonces restamos los segundos de apertura para que no deje entrar.
                    if (!Force)
                        TimeForcing -= RoleplayManager.TimeForRobHouses;

                    if (House != null)
                        House.ForceHouse(TimeForcing);

                    Room Room = RoleplayManager.GenerateRoom(base.Client.GetRoomUser().RoomId);
                    if(Room != null)
                        PlusEnvironment.GetGame().GetClientManager().RadioAlert("¡Atención a todas las autoridades! Nos han reportado un robo en domicilio privado en " + Room.Name, null, true);
                    base.Client.GetPlay().HouseToForce = null;
                    base.Client.GetPlay().IsForcingHouse = false;
                }
                #endregion

                #region Minero
                if (base.Client.GetPlay().IsMinerLoading)
                {
                    RoleplayManager.Shout(base.Client, "*Agarra una roca con sus manos*", 5);
                    base.Client.SendWhisper("Deja la roca en la procesadora que se encuentra afuera de la Mina.", 1);
                    base.Client.GetPlay().MinerRock = true;
                    base.Client.GetPlay().IsMinerLoading = false;

                    if (base.Client.GetRoomUser().CurrentEffect == EffectsList.Pickaxe)
                        base.Client.GetRoomUser().ApplyEffect(0);
                }
                #endregion

                #region Mecánico
                if (base.Client.GetPlay().IsMecLoading)
                {
                    GameClient Cliente = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(base.Client.GetPlay().MecUserToRepair);
                    if (Cliente != null)
                    {
                        List<VehiclesOwned> VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByFurniId(base.Client.GetPlay().MecCarToRepair);
                        if (VO != null && VO.Count > 0)
                        {
                            int Price = base.Client.GetPlay().MecPriceTo;
                            RoleplayManager.Shout(base.Client, "*Repara el vehículo y cierra el capó*", 5);
                            base.Client.SendWhisper("¡Vehículo reparado! Has ganado $" + Price, 1);

                            RoleplayManager.JobSkills(base.Client, base.Client.GetPlay().JobId, base.Client.GetPlay().MecLvl, base.Client.GetPlay().MecXP);

                            base.Client.GetPlay().MecParts -= base.Client.GetPlay().MecPartsTo;
                            RoleplayManager.UpdateVehicleState(base.Client.GetPlay().MecCarToRepair, base.Client.GetPlay().MecNewState);
                            RoleplayManager.UpdateVehicleStat(base.Client.GetPlay().MecCarToRepair, "life", 100);
                            VO[0].State = base.Client.GetPlay().MecNewState;
                            VO[0].CarLife = 100;

                            // Cobro $
                            Cliente.GetHabbo().Credits -= Price;
                            Cliente.GetHabbo().UpdateCreditsBalance();
                            base.Client.GetHabbo().Credits += Price;
                            base.Client.GetPlay().MoneyEarned += Price;
                            base.Client.GetHabbo().UpdateCreditsBalance();
                        }
                        else
                        {
                            base.Client.SendWhisper("No se pudo reparar el vehículo. ((Contacta con un administrador))", 1);
                        }
                        base.Client.GetPlay().MecCarToRepair = 0;
                        base.Client.GetPlay().MecPartsTo = 0;
                        base.Client.GetPlay().MecPriceTo = 0;
                        base.Client.GetPlay().MecUserToRepair = 0;
                        base.Client.GetPlay().MecNewState = 0;
                        base.Client.GetPlay().MecRotPosition = 0;
                        base.Client.GetPlay().IsMecLoading = false;
                    }
                    else
                    {
                        base.Client.GetPlay().MecCarToRepair = 0;
                        base.Client.GetPlay().MecPartsTo = 0;
                        base.Client.GetPlay().MecPriceTo = 0;
                        base.Client.GetPlay().MecUserToRepair = 0;
                        base.Client.GetPlay().MecNewState = 0;
                        base.Client.GetPlay().MecRotPosition = 0;
                        base.Client.GetPlay().IsMecLoading = false;
                        base.Client.SendWhisper("¡Oh oh! Al parecer la persona con quien negociabas se ha ido. No se te pagará nada pero tampoco le dejarás su Vehículo Reparado.", 1);
                    }
                }
                #endregion

                #region Farming
                if (base.Client.GetPlay().IsFarming)
                {
                    #region Get Position User Vars
                    RoomUser User = base.Client.GetRoomUser();

                    if (User == null)
                        return;

                    int X = User.X;
                    int Y = User.Y;
                    double Z = User.Z;
                    int Rot = User.RotBody;
                    #endregion
                    Room Room = RoleplayManager.GenerateRoom(User.RoomId);

                    if (!base.Client.GetPlay().WateringCan)
                    {
                        // Colocamos Furni en Sala
                        base.Client.GetPlay().isParking = true;
                        Item Item = RoleplayManager.PlaceItemToRoom(base.Client, 9436, 0, X, Y, Z, Rot, false, User.RoomId, false, "0");
                        base.Client.GetPlay().isParking = false;
                        base.Client.GetPlay().FarmSeeds = false;
                        RoleplayManager.Shout(base.Client, "*Terminó de sembrar unas semillas*", 5);
                        base.Client.SendWhisper("¡Bien hecho! Ahora consigue una regadera en la llave del establo y podrás :regar tus semillas.", 1);
                    }
                    else
                    {
                        Item BTile = null;
                        if(Room != null)
                            BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "easter_c17_carrot" && x.Coordinate == base.Client.GetRoomUser().Coordinate);
                        if (BTile != null)
                        {
                            BTile.ExtraData = "1";
                            BTile.GetZ += 0.1;
                            Room.SendMessage(new ObjectUpdateComposer(BTile, Convert.ToInt32(base.Client.GetHabbo().Id)));
                        }
                        
                        base.Client.GetPlay().WateringCan = false;
                        RoleplayManager.Shout(base.Client, "*Terminó de regar unas semillas*", 5);
                        base.Client.SendWhisper("¡Ahora ya puedes :cosechar tu cultivo!", 1);
                    }
                    base.Client.GetPlay().IsFarming = false;
                }
                #endregion

                #region Turfs
                if (base.Client.GetPlay().TurfCapturing)
                {
                    RoleplayManager.Shout(base.Client, "*Captura el territorio con éxito*", 5);
                    base.Client.GetPlay().TurfCapturing = false;

                    List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(base.Client.GetHabbo().Id);
                    if(MyGang != null && MyGang.Count > 0)
                        RoleplayManager.ClaimTurf(base.Client, base.Client.GetRoomUser().GetRoom(), MyGang[0]);
                }
                #endregion

                #region Call Taxi
                if (base.Client.GetPlay().CallingTaxi && base.Client.GetRoomUser() != null && base.Client.GetRoomUser().GetRoom() != null)
                {
                    RoleplayManager.CallTaxi(base.Client, base.Client.GetRoomUser().GetRoom());
                    base.Client.GetPlay().CallingTaxi = false;
                }
                #endregion

                #region Toggling PSV Mode
                if (base.Client.GetPlay().TogglingPSV)
                {
                    RoleplayManager.TogglePassiveMode(base.Client);
                    base.Client.GetPlay().TogglingPSV = false;
                }
                #endregion

                base.Client.GetPlay().LoadingTimeLeft = 0;
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