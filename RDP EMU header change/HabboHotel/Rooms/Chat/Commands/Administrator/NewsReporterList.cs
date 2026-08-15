using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class NewsReporterList : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_newsreporter"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Muestra un listado de los reporteros de la ciudad."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("reporters"))
                return;
            #endregion

            string Reporters = "";
            Reporters += "============================================\n";
            Reporters += "          Reporteros de la Ciudad        \n";
            Reporters += "============================================\n\n";

            foreach (string line in RoleplayManager.GetReporterList())
            {
                Reporters += line + "\n";
            }

            Session.SendNotifWithScroll(Reporters);
            Session.GetPlay().CooldownManager.CreateCooldown("reporters", 1000, 3);
        }
    }
}