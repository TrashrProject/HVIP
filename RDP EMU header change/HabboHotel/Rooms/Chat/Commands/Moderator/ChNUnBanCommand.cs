using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;


using Plus.HabboHotel.Moderation;

using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class ChNUnBanCommand : IChatCommand
    {

        public string PermissionRequired
        {
            get { return "command_chn_unban"; }
        }

        public string Parameters
        {
            get { return "%username%"; }
        }

        public string Description
        {
            get { return "Desbanea a un usuario del Canal :n"; ; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length != 2)
            {
                Session.SendWhisper("((Ingresa el nombre de usuario a desbanear del Canal :n))", 1);
                return;
            }
            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("((No se encontró registro de ese usuario))", 1);
                return;
            }
            GameClient T = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Habbo.Id);
            if (T == null)
            {
                Session.SendWhisper("((No se encontró registro de ese usuario))", 1);
                return;
            }
            if(Habbo.Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("((No puedes desbanearte a ti mism@))", 1);
                return;
            }
            if (!T.GetPlay().ChNBanned)
            {
                Session.SendWhisper("((Ese usuario no está baneado del Canal :n))", 1);
                return;
            }
            if (Habbo.GetPermissions().HasRight("mod_soft_ban") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("((No puedes desbanear a ese usuario))", 1);
                return;
            }
            #endregion

            #region Execute
            T.GetPlay().ChNBanned = false;
            T.GetPlay().ChNBannedTimeLeft = 0;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_stats` SET `chn_banned` = '0', `chn_banned_time_left` = '0' WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
            }
            Session.SendWhisper("((Has desbaneado a " + Habbo.Username + " del Canal :n))");
            T.SendWhisper("((Te han desbaneado del Canal :n))", 1);
            #endregion
        }
    }
}