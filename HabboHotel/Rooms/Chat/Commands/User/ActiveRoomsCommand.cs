using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class ActiveRoomsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_active_rooms"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Ver las zonas activas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("activerooms"))
                return;
            #endregion

            string Rooms = "";
            Rooms += "============================================\n";
            Rooms += "          Zonas activas de la Ciudad        \n";
            Rooms += "============================================\n\n";

            foreach (Room RoomAct in PlusEnvironment.GetGame().GetRoomManager()._rooms.Values.ToList().OrderByDescending(key => key.UserCount))
            {
                if (RoomAct.UserCount <= 0)
                    continue;

                Rooms += "Zona ID: " + RoomAct.RoomId + "\n";
                Rooms += "Nombre: " + RoomAct.RoomData.Name + "\n";
                Rooms += "Jugadores: " + RoomAct.UserCount + "\n\n";
            }

            Session.SendNotifWithScroll(Rooms);
            Session.GetPlay().CooldownManager.CreateCooldown("activerooms", 1000, 3);
        }
    }
}
