using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.General
{
    class SkillsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_skills"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Te muestra tu progreso para trabajos secundarios."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Session.GetPlay().TryGetCooldown("skills"))
                return;
            #endregion

            #region Execute
            string str = "";
            str += "\n============================================\n                  Habilidades de Trabajos \n============================================\n";

            str += "Trabajo de Camionero (Nivel "+ Session.GetPlay().CamLvl +": - Progreso "+ Session.GetPlay().CamXP + "/50)\n";
            str += "Trabajo de Armero (Nivel "+ Session.GetPlay().ArmLvl +": - Progreso "+ Session.GetPlay().ArmXP + "/50)\n";
            str += "Trabajo de Mecánico (Nivel "+ Session.GetPlay().MecLvl +": - Progreso "+ Session.GetPlay().MecXP + "/50)\n";
            str += "Trabajo de Basurero (Nivel "+ Session.GetPlay().BasuLvl +": - Progreso "+ Session.GetPlay().BasuXP + "/50)\n";
            str += "Trabajo de Ladrón (Nivel "+ Session.GetPlay().LadronLvl +": - Progreso "+ Session.GetPlay().LadronXP + "/50)\n";
            str += "Trabajo de Minero (Nivel "+ Session.GetPlay().MinerLvl +": - Progreso "+ Session.GetPlay().MinerXP + "/50)\n";

            Session.SendNotifWithScroll(str);
            //Session.SendMessage(new MOTDNotificationComposer(str));
            Session.GetPlay().CooldownManager.CreateCooldown("skills", 1000, 3);
            #endregion
        }
    }
}