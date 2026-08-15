using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class StaffAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_staff_alert"; }
        }

        public string Parameters
        {
            get { return "%message%"; }
        }

        public string Description
        {
            get { return "Envía una alerta a todos los miemrbos del Staff online."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar un mensaje.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            PlusEnvironment.GetGame().GetClientManager().StaffAlert(new MOTDNotificationComposer("Mensaje al Administrativo:\r\r" + Message + "\r\n" + "- " + Session.GetHabbo().Username));
            return;
        }
    }
}