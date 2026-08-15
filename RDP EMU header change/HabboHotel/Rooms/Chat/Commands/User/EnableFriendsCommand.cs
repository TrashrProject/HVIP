using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;



namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class EnableFriendsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_enable_friends"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva las solicitudes de contacto."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().AllowFriendRequests = !Session.GetHabbo().AllowFriendRequests;
            Session.SendWhisper("Ahora" + (Session.GetHabbo().AllowFriendRequests == true ? "recibes" : "no recibes") + " solicitudes de contacto.", 1);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if(Session.GetHabbo().AllowFriendRequests)
                    dbClient.SetQuery("UPDATE `users` SET `block_newfriends` = '0' WHERE `id` = '" + Session.GetHabbo().Id + "'");
                else
                    dbClient.SetQuery("UPDATE `users` SET `block_newfriends` = '1' WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.RunQuery();
            }
        }
    }
}