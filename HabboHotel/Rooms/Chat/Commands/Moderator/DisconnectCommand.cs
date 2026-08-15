using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class DisconnectCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disconnect"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Desconecta a un usuario."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Escribe el nombre del usuario que deseas desconectar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("¡Oops! Probablemente el usuario no se encuentre en línea.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendWhisper("No puedes desconectar a un usuario con el mismo rango o superior al tuyo.", 1);
                return;
            }

            /*
            if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tool") && !TargetClient.GetHabbo().GetPermissions().HasRight("mod_disconnect_any"))
            {
                Session.SendWhisper("No tienes permisos para desconectar a un miembro del equipo Staff.", 1);
                return;
            }
            */

            TargetClient.GetConnection().Dispose();
        }
    }
}