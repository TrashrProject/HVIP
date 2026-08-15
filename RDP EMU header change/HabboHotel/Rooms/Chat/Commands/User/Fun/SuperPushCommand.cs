using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class SuperPushCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_super_push"; }
        }

        public string Parameters
        {
            get { return "%target%"; }
        }

        public string Description
        {
            get { return "Empujar a otro usuario hasta 3 cuadros lejos de ti."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingrese el nombre de usuario a empujar.", 1);
                return;
            }

            if (!Room.SPushEnabled && !Room.CheckRights(Session, true) && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
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

            if (!((Math.Abs(TargetUser.X - ThisUser.X) >= 2) || (Math.Abs(TargetUser.Y - ThisUser.Y) >= 2)))
            {
                if (TargetUser.SetX - 1 == Room.GetGameMap().Model.DoorX || TargetUser.SetY - 1 == Room.GetGameMap().Model.DoorY)
                {
                    Session.SendWhisper("No empujes a nadie fuera de la zona.", 1);
                    return;
                }

                if (TargetUser.SetX - 2 == Room.GetGameMap().Model.DoorX || TargetUser.SetY - 2 == Room.GetGameMap().Model.DoorY)
                {
                    Session.SendWhisper("No empujes a nadie fuera de la zona.", 1);
                    return;
                }

                if (TargetUser.SetX - 3 == Room.GetGameMap().Model.DoorX || TargetUser.SetY - 3 == Room.GetGameMap().Model.DoorY)
                {
                    Session.SendWhisper("No empujes a nadie fuera de la zona.", 1);
                    return;
                }

                if (TargetUser.RotBody == 4)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                }

                if (ThisUser.RotBody == 0)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                }

                if (ThisUser.RotBody == 6)
                {
                    TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                }

                if (ThisUser.RotBody == 2)
                {
                    TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                }

                if (ThisUser.RotBody == 3)
                {
                    TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                }

                if (ThisUser.RotBody == 1)
                {
                    TargetUser.MoveTo(TargetUser.X + 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                }

                if (ThisUser.RotBody == 7)
                {
                    TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 3);
                }

                if (ThisUser.RotBody == 5)
                {
                    TargetUser.MoveTo(TargetUser.X - 3, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 3);
                }

                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*Empuja fuertemente a " + Params[1] + "*", 0, 5));
            }
            else
            {
                Session.SendWhisper("¡" + Params[1] + " está demasiado lejos!", 1);
            }
        }
    }
}