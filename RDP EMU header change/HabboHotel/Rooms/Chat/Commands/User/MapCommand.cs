

using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Notifications;


using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Pets;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Polls;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Availability;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Polls.Questions;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class MapCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_map"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Abre el mapa de la ciudad."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            // Enviamos WS de ventana del mapa
            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "map");
        }
    }
}
