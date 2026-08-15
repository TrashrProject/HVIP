using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DNDCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_dnd"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva los mensajes en consola."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().AllowConsoleMessages = !Session.GetHabbo().AllowConsoleMessages;
            Session.SendWhisper("Ahora" + (Session.GetHabbo().AllowConsoleMessages == true ? "puedes" : "no puedes") + " aceptar mensajes en consola.", 1);
        }
    }
}