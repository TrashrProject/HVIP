using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class MoonwalkCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_moonwalk"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Camina hacia atrás al estilo Michael Jackson."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            User.moonwalkEnabled = !User.moonwalkEnabled;

            if (User.moonwalkEnabled)
                Session.SendWhisper("¡Moonwalk activado!", 1);
            else
                Session.SendWhisper("¡Moonwalk desactivado!", 1);
        }
    }
}
