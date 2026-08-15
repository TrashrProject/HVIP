using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Vehicles;
using System.Data;
using System.Text.RegularExpressions;
using Plus.HabboRoleplay.VehicleOwned;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class CloseCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_close"; }
        }
        // :cerrar (auto)
        // :cerrar [casa]
        
        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Interactúa con cosas que puedas abrir o cerrar. (EJ: Autos, Casas, etc)."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            int Dir = 0;
            string toDo = "";

            #region Conditions
            if (Params.Length == 2)
            {
                Dir = 1;
                toDo = Params[1].ToLower();// casa,...
            }
            else if (Params.Length == 1)
            {
                Dir = 2;// :abrir
            }
            else
            {
                Session.SendWhisper("Comando inválido, usa ':ayuda' para ver más información acerca de los comandos.", 1);
                return;
            }

            #region Basic Conditions
            if (Session.GetPlay().Cuffed)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                return;
            }
            if (!Session.GetRoomUser().CanWalk)
            {
                Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                return;
            }
            if (Session.GetPlay().Pasajero)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
                return;
            }
            if (Session.GetPlay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsJailed)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            if (Session.GetPlay().IsDying)
            {
                Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                return;
            }
            #endregion
            
            if (Session.GetPlay().TryGetCooldown("close", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            if(Dir == 1)// Casa,...
            {
                switch (toDo)
                {
                    #region Casa
                    case "casa":
                        {
                            if (Session.GetPlay().DrivingCar)
                            {
                                Session.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                                return;
                            }
                            var HouseInside = PlusEnvironment.GetGame().GetHouseManager().GetHouseByInsideRoom(Room.Id);

                            if (HouseInside != null)// Estamos dentro de una casa
                            {
                                if (HouseInside.OwnerId != Session.GetHabbo().Id)
                                {
                                    Session.SendWhisper("Solo el propietario de la Casa puede abrir o cerrar su entrada.", 1);
                                    return;
                                }
                                if (HouseInside.IsLocked)
                                {
                                    Session.SendWhisper("Tu casa ya está cerrada.", 1);
                                    return;
                                }
                                HouseInside.CloseHouse(Session);
                            }
                            else // Estamos en un exterior
                            {
                                var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByPosition(Room.Id, Session.GetRoomUser().X, Session.GetRoomUser().Y, Session.GetRoomUser().Z);
                                if (House == null)
                                {
                                    Session.SendWhisper("Debes estar frente a la puerta de tu casa para hacer eso.", 1);
                                    return;
                                }
                                if (House.OwnerId != Session.GetHabbo().Id)
                                {
                                    Session.SendWhisper("No puedes abrir Casas ajenas.", 1);
                                    return;
                                }
                                if (House.IsLocked)
                                {
                                    Session.SendWhisper("Tu casa ya está cerrada.", 1);
                                    return;
                                }
                                House.CloseHouse(Session);
                            }

                            RoleplayManager.Shout(Session, "*Cierra la puerta de su Casa*", 5);
                            Session.GetPlay().CooldownManager.CreateCooldown("close", 1000, 5);
                            break;
                        }
                    #endregion

                    default:
                        Session.SendWhisper("Comando inválido. Usa 'ayuda' para recibir más información.", 1);
                        break;
                }
            }
            else if(Dir == 2)// Abrir/Cerrar
            {
                List<VehiclesOwned> VO = null;

                // Si está conduciendo, obtiene info del auto desde su variable DrivingCarId (Diccionario)
                if (Session.GetPlay().DrivingCar)
                {
                    VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedList(Session.GetPlay().DrivingCarId);
                }
                // Si no está conduciendo, obtiene info del auto desde el furni encima y consulta el itemid al diccionario
                else
                {
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
                        Session.SendWhisper("¡Debes estar sobre un vehículo para hacer eso!", 1);
                        return;
                    }
                    #endregion

                    VO = PlusEnvironment.GetGame().GetVehiclesOwnedManager().getVehiclesOwnedByFurniId(itemfurni);
                }
                if (VO == null || VO.Count <= 0)
                {
                    Session.SendWhisper("((No se encontró información del vehículo))", 1);
                    return;
                }
                if (VO[0].OwnerId != Session.GetHabbo().Id)
                {
                    Session.SendWhisper("Este vehículo no te pertenece.", 1);
                    return;
                }
                if (!VO[0].Traba)
                {
                    Session.SendWhisper("Este vehículo no tiene traba de seguridad. Compra una en un 24/7.", 1);
                    return;
                }

                int State = VO[0].State;
                //state = 0 -> Normal
                //state = 1 -> Bloqueado con traba
                //state = 2 -> No traba y Averiado
                //state = 3 -> Con traba y Averiado
                //state = 4 -> Grua
                if (State == 1 || State == 3)
                {
                    Session.SendWhisper("El vehículo ya se encuentra cerrado.", 1);
                    return;
                }
                int newState = (State == 0) ? 1 : 3;

                VO[0].State = newState;// Actualizamos State en Diccionario
                RoleplayManager.UpdateVehicleState(VO[0].FurniId, newState);// Actualizamos en DB
                RoleplayManager.Shout(Session, "*Cierra las puertas de su Vehículo*", 5);
                Session.GetPlay().CooldownManager.CreateCooldown("close", 1000, 5);
            }
            #endregion
        }
    }
}