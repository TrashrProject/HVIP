using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class MyNumberCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_my_number"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Mira tu número telefónico."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetPlay().Phone == 0)
            {
                Session.SendWhisper("No tienes ningún teléfono comprado para hacer eso.", 1);
                return;
            }

            Session.SendWhisper("Número Telefónico: " + Session.GetPlay().PhoneNumber, 1);
        }
    }
}