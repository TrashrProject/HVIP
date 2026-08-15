using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;


namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class UnmuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_unmute"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Desmutea a un usuario muteado."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingresa el nombre del usuario a demsmutear.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("No se pudo encontrar a ese usuario.", 1);
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `time_muted` = '0' WHERE `id` = '" + TargetClient.GetHabbo().Id + "' LIMIT 1");
            }

            TargetClient.GetHabbo().TimeMuted = 0;
            TargetClient.SendNotification("Has sido desmuteado por " + Session.GetHabbo().Username);
            Session.SendWhisper("Has desmuteado a " + TargetClient.GetHabbo().Username, 1);
        }
    }
}