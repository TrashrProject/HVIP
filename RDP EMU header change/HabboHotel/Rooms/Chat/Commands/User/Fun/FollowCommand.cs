using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class FollowCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_follow"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Seguir a una persona en específico."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir el nombre de una persona.",1 );
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().CurrentRoom == Session.GetHabbo().CurrentRoom)
            {
                Session.SendWhisper("¡Abre tus ojos! " + TargetClient.GetHabbo().Username + " está donde tú.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("¿Es en serio?...", 1);
                return;
            }

            if (!TargetClient.GetHabbo().InRoom)
            {
                Session.SendWhisper("Esa persona no se encuentra en ninguna zona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().CurrentRoom.Access != RoomAccess.OPEN && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Al parecer no es posible seguir a esa persona en donde se encutre.", 1);
                return;
            }

            Session.GetHabbo().PrepareRoom(TargetClient.GetHabbo().CurrentRoom.RoomId, "");
        }
    }
}
