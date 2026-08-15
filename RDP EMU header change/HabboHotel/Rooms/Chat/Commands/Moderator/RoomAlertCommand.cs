using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class RoomAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room_alert"; }
        }

        public string Parameters
        {
            get { return "%message%"; }
        }

        public string Description
        {
            get { return "Envía una alerta en esta zona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar un mensaje.", 1);
                return;
            }

            if(!Session.GetHabbo().GetPermissions().HasRight("mod_alert")/* && Room.OwnerId != Session.GetHabbo().Id*/)
            {
                Session.SendWhisper("No tienes permitido hacer eso.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUser == null || RoomUser.GetClient() == null || Session.GetHabbo().Id == RoomUser.UserId)
                    continue;

                RoomUser.GetClient().SendNotification(Session.GetHabbo().Username + " ha enviado una alerta en su zona con el siguiente mensaje:\n\n" + Message);
            }
            Session.SendWhisper("Alerta enviada exitosamente.", 1);
        }
    }
}
