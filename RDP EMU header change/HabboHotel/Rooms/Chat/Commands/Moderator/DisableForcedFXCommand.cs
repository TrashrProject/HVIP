using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class DisableForcedFXCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_forced_effects"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva la habilidad de permitir efectos forzados en tu personaje."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            Session.GetHabbo().DisableForcedEffects = !Session.GetHabbo().DisableForcedEffects;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `disable_forced_effects` = @DisableForcedEffects WHERE `id` = '" + Session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("DisableForcedEffects", (Session.GetHabbo().DisableForcedEffects == true ? 1 : 0).ToString());
                dbClient.RunQuery();
            }

            Session.SendWhisper("Efectos forzados " + (Session.GetHabbo().DisableForcedEffects == true ? "desactivados." : "activados."), 1);
        }
    }
}
