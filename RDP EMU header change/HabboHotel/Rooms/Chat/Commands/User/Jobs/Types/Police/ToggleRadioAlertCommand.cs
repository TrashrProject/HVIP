using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ToggleRadioAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_toggle_radio_alerts"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Enciende o apaga tu Radio comuniador."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);

            if (MyGang == null || MyGang.Count <= 0)
            {
                Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);

                if (Job == null)
                {
                    Session.SendWhisper("¡No tienes ningún empleo en el cual puedas usar el radio!", 1);
                    return;
                }

                if (Job.Id <= 0)
                {
                    Session.SendWhisper("¡No tienes ningún empleo en el cual puedas usar el radio!", 1);
                    return;
                }

                if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "radio") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                {
                    Session.SendWhisper("¡Tu empleo no requiere uso del Radio!", 1);
                    return;
                }
            }

            Session.GetPlay().DisableRadio = !Session.GetPlay().DisableRadio;
            Session.SendWhisper("Has " + (Session.GetPlay().DisableRadio ? "encendido" : "apagado") + " tu radio comunicador.", 1);
        }
    }
}
