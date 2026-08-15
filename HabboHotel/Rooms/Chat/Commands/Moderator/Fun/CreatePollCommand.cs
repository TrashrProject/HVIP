using Plus.HabboHotel.Rooms.Chat.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rewards.Rooms.Chat.Commands.Moderator.Fun
{
    class CreatePollCommand : IChatCommand
    {

        public string PermissionRequired
        {
            get { return "command_createpoll"; }
        }

        public string Parameters
        {
            get { return "%question%"; }
        }

        public string Description
        {
            get { return "Crea una encuesta a todos en la zona."; }
        }

        public void Execute(GameClients.GameClient Session, Plus.HabboHotel.Rooms.Room Room, string[] Params)
        {
            if(Params.Length < 1)
            {
                Session.SendWhisper("Debes ingresar una pregunta para la encuesta.", 1);
                return;
            }
            string Message = CommandManager.MergeParams(Params, 1);
            Session.GetHabbo().CurrentRoom.startQuestion(Message);
            return;
        }
    }
}

