using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Incoming.Groups;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Cache;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using System.Text.RegularExpressions;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Users;
using System.Collections.Concurrent;
using Group = Plus.HabboHotel.Groups.Group;
using Plus.HabboRoleplay.Houses;
using System.Data;
using Plus.Communication.Packets.Outgoing.Rooms.Session;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// CommandsWebEvent class.
    /// </summary>
    class CommandsWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            switch (Action)
            {
                #region Police CMDS
                case "show_police_cmds":
                    {
                        Socket.Send("compose_commands|show_police_cmds|");
                    }
                    break;
                case "hide_police_cmds":
                    {
                        Socket.Send("compose_commands|hide_police_cmds|");
                    }
                    break;
                #endregion

                #region Map
                case "map":
                    {
                        Socket.Send("compose_commands|map|");
                    }
                    break;
                #endregion

                #region Open
                case "open":
                    {
                        Socket.Send("compose_commands|open|");
                    }
                    break;
                #endregion

                #region Jobs
                case "jobs":
                    {
                        Socket.Send("compose_commands|jobs|");

                        #region Tutorial Step Check
                        if (Client.GetPlay().TutorialStep == 32)
                        {
                            Socket.Send("compose_tutorial|32");
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region Houses
                case "houses":
                    {
                        Socket.Send("compose_commands|houses|");
                    }
                    break;
                #endregion

                #region Vehicles
                case "vehicles":
                    {
                        Socket.Send("compose_commands|vehicles|");
                    }
                    break;
                #endregion

                #region Empresas
                case "bussines":
                    {
                        Socket.Send("compose_commands|bussines|");
                    }
                    break;
                #endregion

                #region Terrenos
                case "terrains":
                    {
                        Socket.Send("compose_commands|terrains|");
                    }
                    break;
                #endregion

                #region Marihauana
                case "marijane":
                    {
                        Socket.Send("compose_commands|marijane|");
                    }
                    break;
                #endregion

                #region Bandas
                case "gangs":
                    {
                        Socket.Send("compose_commands|gangs|");
                    }
                    break;
                #endregion

                default:
                    break;
            }
        }
    }
}
