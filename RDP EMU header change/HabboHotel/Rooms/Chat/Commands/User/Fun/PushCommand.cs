using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class PushCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_push"; }
        }

        public string Parameters
        {
            get { return "%target%"; }
        }

        public string Description
        {
            get { return "Empuja a un usuario."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            if (!Room.PushEnabled && !Session.GetHabbo().GetPermissions().HasRight("room_override_custom_config"))
            {
                Session.SendWhisper("No se puede hacer eso en esta zona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);
            if (TargetUser == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona en esta zona.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                return;
            }

            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendWhisper("No puedes empujar a un usuario con el mismo rango o superior al tuyo.", 1);
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && TargetClient.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes hacerle eso a una persona en modo pasivo.", 1);
                return;
            }

            /*
            if (TargetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("No puedes empujar a ese usuario.", 1);
                return;
            }
            */

            if (TargetUser.TeleportEnabled)
            {
                Session.SendWhisper("No puedes empujar a ese usuario en este momento.", 1);
                return;
            }

            if (TargetClient.LoggingOut)
            {
                Session.SendWhisper("((Esta persona se encuentra desconectándose.))", 1);
                return;
            }

            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (!((Math.Abs(TargetUser.X - ThisUser.X) >= 2) || (Math.Abs(TargetUser.Y - ThisUser.Y) >= 2)))
            {
                if (TargetUser.SetX - 1 == Room.GetGameMap().Model.DoorX)
                {
                    Session.SendWhisper("No empujes a nadie afuera de una zona.", 1);
                    return;
                }

                if (TargetUser.RotBody == 4)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 1);
                }

                if (ThisUser.RotBody == 0)
                {
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 1);
                }

                if (ThisUser.RotBody == 6)
                {
                    TargetUser.MoveTo(TargetUser.X - 1, TargetUser.Y);
                }

                if (ThisUser.RotBody == 2)
                {
                    TargetUser.MoveTo(TargetUser.X + 1, TargetUser.Y);
                }

                if (ThisUser.RotBody == 3)
                {
                    TargetUser.MoveTo(TargetUser.X + 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 1);
                }

                if (ThisUser.RotBody == 1)
                {
                    TargetUser.MoveTo(TargetUser.X + 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 1);
                }

                if (ThisUser.RotBody == 7)
                {
                    TargetUser.MoveTo(TargetUser.X - 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y - 1);
                }

                if (ThisUser.RotBody == 5)
                {
                    TargetUser.MoveTo(TargetUser.X - 1, TargetUser.Y);
                    TargetUser.MoveTo(TargetUser.X, TargetUser.Y + 1);
                }

                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*Empuja a " + Params[1] + "*", 0, 5));
            }
            else
            {
                Session.SendWhisper("" + Params[1] + " está demasiado lejos.", 1);
            }
        }
    }
}