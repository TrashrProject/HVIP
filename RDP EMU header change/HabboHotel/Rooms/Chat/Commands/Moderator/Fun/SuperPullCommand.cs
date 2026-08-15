using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    class SuperPullCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_super_pull"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Atrae a una persona desde cualquier distancia."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor ingresa el nombre de usuario a empujar.", 1);
                return;
            }

            if (!Room.SPullEnabled && !Room.CheckRights(Session, true) && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("No se tiene permitido hacer eso en esta zona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("No se encontró a esa persona.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper("No se puedo encontrar a esa persona en esta zona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mismo.", 1);
                return;
            }

            if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("No puedes empujar a esta persona.", 1);
                return;
            }

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("No puedes empujar a esa persona en este momento.", 1);
                return;
            }

            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (ThisUser.SetX - 1 == Room.GetGameMap().Model.DoorX)
            {
                Session.SendWhisper("No empujes a nadie fuera de la zona.", 1);
                return;
            }

            if (ThisUser.RotBody % 2 != 0)
                ThisUser.RotBody--;
            if (ThisUser.RotBody == 0)
                TargetUser.MoveTo(ThisUser.X, ThisUser.Y - 1);
            else if (ThisUser.RotBody == 2)
                TargetUser.MoveTo(ThisUser.X + 1, ThisUser.Y);
            else if (ThisUser.RotBody == 4)
                TargetUser.MoveTo(ThisUser.X, ThisUser.Y + 1);
            else if (ThisUser.RotBody == 6)
                TargetUser.MoveTo(ThisUser.X - 1, ThisUser.Y);

            Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*Atrae a " + Params[1] + " desde lejos*", 0, 5));
            return;
        }
    }
}