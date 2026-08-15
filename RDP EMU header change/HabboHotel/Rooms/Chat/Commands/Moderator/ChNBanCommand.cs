using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;


using Plus.HabboHotel.Moderation;

using Plus.Database.Interfaces;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class ChNBanCommand : IChatCommand
    {

        public string PermissionRequired
        {
            get { return "command_chn_ban"; }
        }

        public string Parameters
        {
            get { return "%username% %length%"; }
        }

        public string Description
        {
            get { return "Banea a un usuario temporalmente del Canal :n"; ; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length != 3)
            {
                Session.SendWhisper("((Ingresa el nombre de usuario y el tiempo en minutos del baneo para el Canal :n))", 1);
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
                Session.SendWhisper("((No puedes banearte a ti mism@))", 1);
                return;
            }
            if (Habbo.GetPermissions().HasRight("mod_soft_ban") && !Session.GetHabbo().GetPermissions().HasRight("mod_ban_any"))
            {
                Session.SendWhisper("((No puedes banear a ese usuario))", 1);
                return;
            }
            int Time = 0;
            if (!int.TryParse(Params[2], out Time))
            {
                Session.SendWhisper("((Ingresa un número válido))", 1);
                return;
            }
            if(Time < RoleplayManager.MinBanChNTime)
            {
                Session.SendWhisper("((El tiempo mínimo a Banear es de 10 minutos))", 1);
                return;
            }
            if (Time > RoleplayManager.MaxBanChNTime)
            {
                Session.SendWhisper("((El tiempo máximo a Banear es de 4320 minutos))", 1);
                return;
            }
            #endregion

            #region Execute
            T.GetPlay().ChNBanned = true;
            T.GetPlay().ChNBannedTimeLeft = Time;
            T.GetPlay().TimerManager.CreateTimer("chnban", 1000, false);
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `play_stats` SET `chn_banned` = '1', `chn_banned_time_left` = '"+ Time +"' WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
            }
            PlusEnvironment.GetGame().GetClientManager().AlertMessage(Session.GetHabbo().Username + " ha baneado a " + Habbo.Username + " del Canal :n por mal uso.", "[Canal :n]");
            T.SendWhisper("((Te han baneado del Canal :n por mal usar. No podrás usarlo durante "+ Time +" minuto(s) ))", 1);
            //Session.SendWhisper("Has baneado el Canal :n de '" + Habbo.Username + "' por " + Time + " minuto(s)", 1);
            #endregion
        }
    }
}