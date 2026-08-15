using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Combat;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Combat
{
    class YCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_chanel"; }
        }

        public string Parameters
        {
            get { return "%msg%"; }
        }

        public string Description
        {
            get { return "Para hablar en el canal para rolear acciones."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("((Ingresa una acción a rolear))", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("ycommand", true))
                return;
            #endregion

            #region Execute
            RoleplayManager.Shout(Session, "*"+ CommandManager.MergeParams(Params, 1) +"*", 5);
            Session.GetPlay().CooldownManager.CreateCooldown("ycommand", 1000, 2);
            #endregion
        }
    }
}