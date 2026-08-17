using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class RoomIdsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_roomids"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Muestra la lista de todas las salas con su id."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("roomids"))
                return;
            #endregion

            string Rooms = "";
            Rooms += "============================================\n";
            Rooms += "         Todas las Zonas de la Ciudad       \n";
            Rooms += "============================================\n\n";

            foreach (RoomData RoomAct in PlusEnvironment.GetGame().GetRoomManager()._loadedRoomData.Values.ToList().OrderBy(key => key.Id))
            {
                Rooms += "Zona ID: " + RoomAct.Id + "\n";
                Rooms += "Nombre: " + RoomAct.Name + "\n";
                Rooms += "Jugadores: " + RoomAct.UsersNow + "\n\n";
            }

            Session.SendNotifWithScroll(Rooms);
            Session.GetPlay().CooldownManager.CreateCooldown("roomids", 1000, 3);
        }
    }
}
