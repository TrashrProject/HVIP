using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class ChatHTMLSizeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_HTMLsize"; }
        }
        public string Parameters
        {
            get { return "%size%"; }
        }
        public string Description
        {
            get { return "Cambia el tamaño de tu nombre."; }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir un tamaño entre 1 y 20.", 1);
                return;
            }
            string chatColour = Params[1];

            int chatsize;
            bool isNumeric = int.TryParse(chatColour, out chatsize);
            if (isNumeric)
            {
                if(Session.GetHabbo().chatHTMLColour == null || Session.GetHabbo().chatHTMLColour == String.Empty)
                {
                    Session.GetHabbo().chatHTMLColour = "000000";
                }
                switch (chatsize)
                {
                    case 12:
                        Session.GetHabbo().chatHTMLSize = 12;
                        Session.SendWhisper("Tu tamaño de nombre ha vuelto a la normalidad.", 1);
                        break;
                    default:
                        bool isValid = true;
                        if (chatsize < 1)
                        {
                            isValid = false;
                        }

                        if (chatsize > 20 && Session.GetHabbo().Rank < 6)
                        {
                            isValid = false;
                        }
                        if (isValid)
                        {
                            Session.SendWhisper("El tamaño ha sido cambiado a " + chatsize + ".", 1);
                            Session.GetHabbo().chatHTMLSize = chatsize;
                        }
                        else
                        {
                            Session.SendWhisper("Tamaño inválido, debe ser un número entre 1 y 20.", 1);
                        }

                        break;
                }
            }
            else
            {
                Session.SendWhisper("Tamaño inválido, debe ser un número entre 1 y 20.", 1);
            }
            return;
        }
    }
}
