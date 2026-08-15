using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class SpinCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_spin"; }
        }
        public string Parameters
        {
            get { return ""; }
        }
        public string Description
        {
            get { return "Spin in a circle"; }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;
            User.ApplyEffect(500);
            Session.SendWhisper("let me spin in that pussy ;)");
        }
    }
}