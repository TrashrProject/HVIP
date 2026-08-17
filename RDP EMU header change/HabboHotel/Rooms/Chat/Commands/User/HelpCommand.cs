
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
    class HelpCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_help"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Recibe ayuda específica sobre los comandos."; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length == 2)
            {
                switch (Params[1].ToLower())
                {
                    #region Trabajos
                    case "trabajos":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "jobs");
                        break;
                    #endregion

                    #region Casas
                    case "casas":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "houses");
                        break;
                    #endregion

                    #region Vehículos
                    case "vehiculos":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "vehicles");
                        break;
                    #endregion

                    #region Empresas
                    case "empresas":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "bussines");
                        break;
                    #endregion

                    #region Terrenos
                    case "terrenos":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "terrains");
                        break;
                    #endregion

                    #region Marihuana
                    case "marihuana":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "marijane");
                        break;
                    #endregion

                    #region Bandas
                    case "bandas":
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "gangs");
                        break;
                    #endregion

                    #region Default
                    default:
                        Session.SendWhisper("((Comando inválido. Usa :ayuda para recibir más información))", 1);
                    break;
                    #endregion
                }
            }
            else
            {
                // Enviamos WS de ventana de comandos.
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_commands", "open");
            }
        }
    }
}
