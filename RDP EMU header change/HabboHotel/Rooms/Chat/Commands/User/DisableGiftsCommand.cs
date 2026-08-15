using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;



namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class DisableGiftsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_disable_gifts"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva la habilidad de recibir regalos."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetHabbo().AllowGifts = !Session.GetHabbo().AllowGifts;
            Session.SendWhisper("Ahora " + (Session.GetHabbo().AllowGifts == true ? "puedes" : "no puedes") + " aceptar regalos.", 1);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `allow_gifts` = @AllowGifts WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.AddParameter("AllowGifts", PlusEnvironment.BoolToEnum(Session.GetHabbo().AllowGifts));
                dbClient.RunQuery();
            }
        }
    }
}