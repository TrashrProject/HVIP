using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class AlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert_user"; }
        }

        public string Parameters
        {
            get { return "%username% %Messages%"; }
        }

        public string Description
        {
            get { return "Alerta con un mensaje a un usuario específico."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingresa el nombre de usuario a alertar.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            if (TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Consíguete una vida.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 2);

            TargetClient.SendNotification(Session.GetHabbo().Username + " te ha mandado la siguiente alerta:\n\n" + Message);
            Session.SendWhisper("Alerta enviada a " + TargetClient.GetHabbo().Username + " correctamente.");

        }
    }
}
