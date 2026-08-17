
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
    class InfoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_info"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Mira información del Servidor."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            TimeSpan Uptime = DateTime.Now - PlusEnvironment.ServerStarted;
            int OnlineUsers = PlusEnvironment.GetGame().GetClientManager().Count;
            int RoomCount = PlusEnvironment.GetGame().GetRoomManager().Count;
            string HotelName = PlusEnvironment.GetConfig().data["hotel.link"];


            Session.SendMessage(new RoomNotificationComposer("Acerca del Servidor",
                 "<font color=\"#0489B1\" size=\"18\">RDP Emulator - Retro Development Project</font>\n\n" +
                 "<b>Créditos a:</b>\n" +
                 "\t- <font color=\"#389DDF\"><b>Jeihden (Developer)\n" +
                 "\t- <font color=\"#389DDF\"><b>Zedd (Developer)\n\n" +
                 "<b>Agradecimientos a:</b>\n" +
                 "\t- <font color=\"#df3838\"><b>Sledmore (Developer)\n" +
                 "\t- Plus Emulator Devs\n" +
                 "\t- Butterfly Emulator Devs</b></font>\n" +
                 "\t- Shock Development Team\n\n" +
                 "<b>Información Actual</b>:\n" +
                 "\t- Usuarios conectados: " + OnlineUsers + "\n" +
                 "\t- Zonas Cargadas: " + RoomCount + "\n" +
                 "\t- Tiempo activo: " + Uptime.Days + " día(s), " + Uptime.Hours + " hora(s) y " + Uptime.Minutes + " minuto(s).\n\n" +
                 "<b>Licencia: </b>\n" +
                 "\t <font color=\"#0489B1\"><b>" + HotelName + "</b></font>", "plus", ""));

        }
    }
}
