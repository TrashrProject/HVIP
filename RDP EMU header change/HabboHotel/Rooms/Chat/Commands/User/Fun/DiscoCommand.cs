using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class DiscoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disco"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Easter Egg"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Room != null || !Room.CheckRights(Session))
            {
                Room.DiscoMode = !Room.DiscoMode;

                if (Room.DiscoMode)
                    Session.SendWhisper("Discomode activado.", 1);
                else
                    Session.SendWhisper("Discomode desactivado.", 1);
            }
        }
    }
}