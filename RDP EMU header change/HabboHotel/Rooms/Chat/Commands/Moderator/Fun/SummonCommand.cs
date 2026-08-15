using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    class SummonCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_summon"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Trae a un usuario a tu zona actual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir el nombre de la persona.", 1);
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

            if (TargetClient.GetPlay().TurfCapturing)
            {
                Session.SendWhisper("Esa persona se encuentra capturando un barrio. Espera a que termine o síguela.", 1);
                return;
            }

            //RoleplayManager.SendUserOld(TargetClient, Room.Id, "");
            TargetClient.GetHabbo().RoomAuthOk = true;
            TargetClient.GetHabbo().PrepareApartment(Room.Id, "");
            TargetClient.SendNotification("Has sido atraíd@ por " + Session.GetHabbo().Username);
            
            /*
            if (!TargetClient.GetHabbo().InRoom)
                RoleplayManager.SendUser(TargetClient, Room.Id, "");
            //TargetClient.SendMessage(new RoomForwardComposer(Session.GetHabbo().CurrentRoomId));
            else
                TargetClient.GetHabbo().PrepareRoom(Session.GetHabbo().CurrentRoomId, "");
            */
        }
    }
}