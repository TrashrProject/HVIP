using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class BackupCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_backup"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Solicita refuerzos policiacos."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);

            if (Job == null)
            {
                Session.SendWhisper("¡No tienes ningún empleo!", 1);
                return;
            }

            if (Job.Id <= 0)
            {
                Session.SendWhisper("¡No tienes ningún empleo!", 1);
                return;
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "radio") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un Oficial de Policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }

            if (Session.GetPlay().TryGetCooldown("police_backup"))
                return;

            PlusEnvironment.GetGame().GetClientManager().JailAlert(Session.GetHabbo().Username + " está solicitando refuerzos en " + Room.Name + " ¡Ve allí rápidamente!");
            Session.GetPlay().CooldownManager.CreateCooldown("police_backup", 1000, 5);
        }
    }
}
