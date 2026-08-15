using Plus.HabboHotel.Rooms.Chat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rewards.Rooms.Chat.Commands.Moderator.Fun
{
    class EndPollCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_endpoll"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Finaliza una encuesta activa en la zona."; }
        }

        public void Execute(GameClients.GameClient Session, Plus.HabboHotel.Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().CurrentRoom.endQuestion();
            return;
        }
    }
}
