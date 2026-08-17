using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;



namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class TradeBanCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_trade_ban"; }
        }

        public string Parameters
        {
            get { return "%target% %length%"; }
        }

        public string Description
        {
            get { return "Banea o desbanea los tradeos de un usuario."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar un nombre de usuario y la duración en días. (min. 1 día, max. 365 días).", 1);
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            if (Convert.ToDouble(Params[2]) == 0)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
                }

                if (Habbo.GetClient() != null)
                {
                    Habbo.TradingLockExpiry = 0;
                    Habbo.GetClient().SendNotification("¡Tus tradeos han sido desbaneados!");
                }

                Session.SendWhisper("Has desbaneado los tradeos de " + Habbo.Username, 1);
                return;
            }

            double Days;
            if (double.TryParse(Params[2], out Days))
            {
                if (Days < 1)
                    Days = 1;

                if (Days > 365)
                    Days = 365;

                double Length = (PlusEnvironment.GetUnixTimestamp() + (Days * 86400));
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '" + Length + "', `trading_locks_count` = `trading_locks_count` + '1' WHERE `user_id` = '" + Habbo.Id + "' LIMIT 1");
                }

                if (Habbo.GetClient() != null)
                {
                    Habbo.TradingLockExpiry = Length;
                    Habbo.GetClient().SendNotification("¡Tus tradeos han sido baneados por " + Days + " día(s)!");
                }

                Session.SendWhisper("Has baneado los tradeos de" + Habbo.Username + " por " + Days + " día(s).", 1);
            }
            else
                Session.SendWhisper("Ingresa un número válido.");
        }
    }
}
