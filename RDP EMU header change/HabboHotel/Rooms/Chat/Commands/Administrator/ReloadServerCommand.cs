using Plus.Communication.Packets.Outgoing.Campaigns;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Rooms.Chat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using System.Drawing;
using Plus.HabboRoleplay.VehicleOwned;
using Plus.Core;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.Utilities;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rewards.Rooms.Chat.Commands.Administrator
{
    class ReloadServerCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_reload_server"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Reiniciar el servidor"; }
        }

        public void Execute(GameClients.GameClient Session, Plus.HabboHotel.Rooms.Room Room, string[] Params)
        {
            // MegaDude
            if (Session.GetHabbo().Rank <= 5 && Session.GetHabbo().Id != 9447)
                return;

            PlusEnvironment.PerformShutDown(true);
        }
    }
}
