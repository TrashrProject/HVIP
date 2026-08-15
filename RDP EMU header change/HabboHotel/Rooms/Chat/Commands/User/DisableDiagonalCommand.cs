using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DisableDiagonalCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disable_diagonal"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva caminar en diagonal en esta zona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
            {
                Session.SendWhisper("Solo el dueño o usuarios con permisos pueden hacer eso.", 1);
                return;
            }

            Room.GetGameMap().DiagonalEnabled = !Room.GetGameMap().DiagonalEnabled;

            if(Room.GetGameMap().DiagonalEnabled)
                Session.SendWhisper("Diagonal activado correctamente.", 1);
            else
                Session.SendWhisper("Diagonal desactivado correctamente.", 1);
        }
    }
}
