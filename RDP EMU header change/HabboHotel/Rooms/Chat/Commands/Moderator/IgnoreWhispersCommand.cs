using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class IgnoreWhispersCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ignore_whispers"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva la habilidad de leer susurros."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().IgnorePublicWhispers = !Session.GetHabbo().IgnorePublicWhispers;
            Session.SendWhisper("Ahora " + (Session.GetHabbo().IgnorePublicWhispers ? "lees" : "no lees") + " susurros.", 1);
        }
    }
}
