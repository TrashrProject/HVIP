using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class RoomUnmuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_unroommute"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desmutear la zona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (!Room.RoomMuted)
            {
                Session.SendWhisper("Esta zona no está muteada.", 1);
                return;
            }

            Room.RoomMuted = false;

            List<RoomUser> RoomUsers = Room.GetRoomUserManager().GetRoomUsers();
            if (RoomUsers.Count > 0)
            {
                foreach (RoomUser User in RoomUsers)
                {
                    if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null || User.GetClient().GetHabbo().Username == Session.GetHabbo().Username)
                        continue;

                    User.GetClient().SendWhisper("La zona ha sido desmuteada.", 1);
                }
            }
        }
    }
}