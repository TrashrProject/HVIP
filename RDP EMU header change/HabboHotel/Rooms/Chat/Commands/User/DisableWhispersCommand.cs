using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DisableWhispersCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disable_whispers"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva la habilidad de recibir susurros."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().ReceiveWhispers = !Session.GetHabbo().ReceiveWhispers;
            Session.SendWhisper("Ahora " + (Session.GetHabbo().ReceiveWhispers ? "puedes" : "no puedes") + " recibir susurros.", 1);
        }
    }
}
