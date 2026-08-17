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
    class EnterCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_enter"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Entra a una Casa."; }
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
            
            if (Session.GetPlay().TryGetCooldown("enter", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByPosition(Room.Id, Session.GetRoomUser().X, Session.GetRoomUser().Y, Session.GetRoomUser().Z);
            if (House == null)
            {
                Session.SendWhisper("Debes estar frente a la puerta de una casa para hacer eso.", 1);
                return;
            }
            if (House.Type == 1)
            {
                if (House.IsLocked)
                {
                    Session.SendWhisper("La casa se encuentra Cerrada.", 1);
                    return;
                }
            }
            else // Si es casa de Robo
            {
                long Seconds = DateTimeOffset.Now.ToUnixTimeSeconds();
                long Total = Seconds - House.Last_Forcing;
                if (Total > RoleplayManager.TimeForRobHouses)// X segundos para cerrarse
                {
                    Session.SendWhisper("La casa se encuentra cerrada. Debes ':forzarcerradura' primero.", 1);
                    return;
                }
            }
            Room InsideRoom = null;
            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(House.InsideRoomId, out InsideRoom))
            { 
                if(InsideRoom.UserIsBanned(Session.GetHabbo().Id))
                {
                    Session.SendNotification("Al parecer has sido expulsad@ de esta Casa/Apartamento. ¡No puedes entrar!");
                    return;
                }
                /*
                foreach (int Id in InsideRoom.BannedUsers().ToList())
                {
                    if(Session.GetHabbo().Id == Id)
                    {
                        Session.SendNotification("Al parecer has sido expulsado de esta Casa/Apartamento. ¡No puedes entrar!");
                        return;
                    }
                }
                */
            }
            RoleplayManager.Shout(Session, "*Ha entrado a la Casa *", 5);
            RoleplayManager.SendUserOld(Session, House.InsideRoomId, "");            
            Session.GetPlay().CooldownManager.CreateCooldown("enter", 1000, 5);
            
            #endregion
        }
    }
}