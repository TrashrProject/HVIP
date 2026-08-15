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
    class PCommand : IChatCommand
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
            get { return "Para hablar en el canal para rolear sobre el entorno."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("((Ingresa una acción sobre el entorno))", 1);
                return;
            }
            if (Session.GetPlay().TryGetCooldown("pcommand", true))
                return;
            #endregion

            #region Execute
            RoleplayManager.Shout(Session, "* "+ CommandManager.MergeParams(Params, 1), 4);
            Session.GetPlay().CooldownManager.CreateCooldown("pcommand", 1000, 2);
            #endregion
        }
    }
}