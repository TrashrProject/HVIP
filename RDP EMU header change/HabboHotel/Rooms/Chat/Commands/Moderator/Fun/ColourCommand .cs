using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class ColourCommand : IChatCommand
    {

        public string PermissionRequired
        {
            get { return "command_colour"; }
        }
        public string Parameters
        {
            get { return "%color%"; }
        }
        public string Description
        {
            get { return "Cambia el color de tu chat."; }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar el color a usar.", 1);
                return;
            }
            string chatColour = Params[1];
            string Colour = chatColour.ToUpper();
            switch (chatColour)
            {
                case "none":
                case "black":
                case "off":
                    Session.GetHabbo().chatColour = "";
                    Session.SendWhisper("Tu Color de Chat ha sido desactivado.", 1);
                    break;
                case "blue":
                case "red":
                case "orange":
                case "yellow":
                case "green":
                case "purple":
                case "pink":
                case "cyan":
                    Session.GetHabbo().chatColour = chatColour;
                    Session.SendWhisper("Tu Color de chat ha sido activado a : " + Colour + ".", 1);
                    break;
                default:
                    Session.SendWhisper("Ese Color de chat: " + Colour + " no existe.", 1);
                    break;
            }
            return;
            }
        }
    }