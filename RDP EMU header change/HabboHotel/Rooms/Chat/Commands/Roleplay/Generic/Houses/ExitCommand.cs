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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Offers
{
    class ExitCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_exit"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Salir de una Casa."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            #region Basic Conditions
            if (Session.GetPlay().DrivingCar)
            {
                Session.SendWhisper("No puedes hacer eso mientras conduces.", 1);
                return;
            }
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

            if (Session.GetPlay().TryGetCooldown("exit", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            var HouseInside = PlusEnvironment.GetGame().GetHouseManager().GetHouseByInsideRoom(Room.Id);

            GameClient Escorted = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetHabbo().Escorting);

            if (HouseInside != null)//Estamos dentro de una casa
            {

                if (Escorted != null)
                {
                    RoleplayManager.Shout(Escorted, "*Ha Salido de la Casa*", 5);
                    // Enviar a la Sala Exterior y Posición de la Puerta
                    Escorted.GetPlay().ExitingHouse = true;
                    Escorted.GetPlay().HouseX = HouseInside.DoorX;
                    Escorted.GetPlay().HouseY = HouseInside.DoorY;
                    Escorted.GetPlay().HouseZ = HouseInside.DoorZ;
                    RoleplayManager.SendUserOld(Escorted, HouseInside.RoomId, "");
                }

                RoleplayManager.Shout(Session, "*Ha Salido de la Casa*", 5);
                // Enviar a la Sala Exterior y Posición de la Puerta
                Session.GetPlay().ExitingHouse = true;
                Session.GetPlay().HouseX = HouseInside.DoorX;
                Session.GetPlay().HouseY = HouseInside.DoorY;
                Session.GetPlay().HouseZ = HouseInside.DoorZ;
                RoleplayManager.SendUserOld(Session, HouseInside.RoomId, "");
                
            }
            else
            {
                var ApartInside = PlusEnvironment.GetGame().GetApartmentOwnedManager().GetApartmentByInsideRoom(Room.Id);

                if(ApartInside == null)
                {
                    Session.SendWhisper("No te encuentras dentro de ninguna casa o apartamento.", 1);
                    return;
                }

                if (Escorted != null)
                {
                    RoleplayManager.Shout(Escorted, "*Ha Salido del apartamento*", 5);
                    RoleplayManager.SendUserOld(Escorted, ApartInside.LobbyId, "");
                }
                
                RoleplayManager.Shout(Session, "*Ha Salido del apartamento*", 5);
                RoleplayManager.SendUserOld(Session, ApartInside.LobbyId, "");
            }

            Session.GetPlay().CooldownManager.CreateCooldown("exit", 1000, 5);
            #endregion
        }
    }
}