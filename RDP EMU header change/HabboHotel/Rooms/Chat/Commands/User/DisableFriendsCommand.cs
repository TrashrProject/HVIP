using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;



namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DisableFriendsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disable_friends"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desactiva las solicitudes de contacto."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().AllowFriendRequests = !Session.GetHabbo().AllowFriendRequests;
            Session.SendWhisper("Ahora " + (Session.GetHabbo().AllowFriendRequests == true ? "recibes" : "no recibes") + " solicitudes de contacto.", 1);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `block_newfriends` = '1' WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.RunQuery();
            }
        }
    }
}