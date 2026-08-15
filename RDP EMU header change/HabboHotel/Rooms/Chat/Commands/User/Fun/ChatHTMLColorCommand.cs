using Plus.HabboHotel.Rooms.Chat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class ChatHTMLColorCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_HTMLcolour"; }
        }
        public string Parameters
        {
            get { return "%color%"; }
        }
        public string Description
        {
            get { return "Cambia el color de tu nombre."; }
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
                    Session.GetHabbo().chatHTMLColour = "";
                    Session.SendWhisper("Tu Color de nombre ha sido desactivado.", 1);
                    break;
                case "blue":
                    Session.GetHabbo().chatHTMLColour = "0052a5";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                case "red":
                    Session.GetHabbo().chatHTMLColour = "e0162b";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                case "orange": 
                    Session.GetHabbo().chatHTMLColour = "ee7b06";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                case "yellow":
                    Session.GetHabbo().chatHTMLColour = "ffff00";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                case "green":
                    Session.GetHabbo().chatHTMLColour = "006600";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                case "purple":
                    Session.GetHabbo().chatHTMLColour = "660066";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                case "pink":
                    Session.GetHabbo().chatHTMLColour = "ff0080";
                    Session.SendWhisper("Tu Color de nombre ha sido activado a : " + Colour + ".", 1);
                    break;
                default:
                    bool isValid = true;
                    if(Colour.Length != 6 )
                    {
                        isValid = false;
                    }

                    int n;
                    bool isNumeric = int.TryParse(Colour, out n);
                    if(isNumeric == false)
                    {
                        isValid = false;
                    }
                    if(isValid)
                    {
                        Session.SendWhisper("El Color de tu nombre ha sido cambiado a #" + Colour + ".", 1);
                        Session.GetHabbo().chatHTMLColour = chatColour;
                    }
                    else
                    {
                        Session.SendWhisper("Ese no es un código valido de color en HEX, debe tener 6 números. ¿No que muy developer?", 1);
                    }
                        
                    break;
            }
            return;
        }
    }
}
