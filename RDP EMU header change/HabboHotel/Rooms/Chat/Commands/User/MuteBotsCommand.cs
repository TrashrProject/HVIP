using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class MuteBotsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mute_bots"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva los dialogos de los bots."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().AllowBotSpeech = !Session.GetHabbo().AllowBotSpeech;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `users` SET `bots_muted` = '" + ((Session.GetHabbo().AllowBotSpeech) ? 1 : 0) + "' WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
            }

            if (Session.GetHabbo().AllowBotSpeech)
                Session.SendWhisper("Ahora no lees los dialogos de los bots.", 1);
            else
                Session.SendWhisper("Ahora lees los dialogos de los bots.", 1);                       
        }
    }
}
