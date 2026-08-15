using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class RoomMuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_roommute"; }
        }

        public string Parameters
        {
            get { return "%message% %reason%"; }
        }

        public string Description
        {
            get { return "Mutea toda una zona con una razón."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor especifica una razón del muteo para mostrar a los usuarios.", 1);
                return;
            }

            if (!Room.RoomMuted)
                Room.RoomMuted = true;

            string Msg = CommandManager.MergeParams(Params, 1);

            List<RoomUser> RoomUsers = Room.GetRoomUserManager().GetRoomUsers();
            if (RoomUsers.Count > 0)
            {
                foreach (RoomUser User in RoomUsers)
                {
                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Username == Session.GetHabbo().Username)
                        continue;

                    User.GetClient().SendWhisper("Esta zona ha sido muteada con la razón: " + Msg, 1);
                }
            }
        }
    }
}
