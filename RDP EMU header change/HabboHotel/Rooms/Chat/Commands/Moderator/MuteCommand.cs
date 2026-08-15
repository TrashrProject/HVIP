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
    class MuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mute"; }
        }

        public string Parameters
        {
            get { return "%username% %time%"; }
        }

        public string Description
        {
            get { return "Mutea a un usuario por un tiempo definido."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingresa un nombre de usuario y cantidad de segundos. (Max. 600 segs).", 1);
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("No se encontró registro de ese usuario.", 1);
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().GetPermissions().HasRight("mod_mute_any"))
            {
                Session.SendWhisper("No puedes mutear a ese usuario.", 1);
                return;
            }

            double Time;
            if (double.TryParse(Params[2], out Time))
            {
                if (Time > 600 && !Session.GetHabbo().GetPermissions().HasRight("mod_mute_limit_override"))
                    Time = 600;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + Time + "' WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
                }

                if (Habbo.GetClient() != null)
                {
                    Habbo.TimeMuted = Time;
                    Habbo.GetClient().SendNotification("Has sido muteado por un Moderador por " + Time + " segundos.");
                }

                Session.SendWhisper("Has mutueado a " + Habbo.Username + " por " + Time + " segundos.", 1);
            }
            else
                Session.SendWhisper("Ingresa un número válido.", 1);
        }
    }
}