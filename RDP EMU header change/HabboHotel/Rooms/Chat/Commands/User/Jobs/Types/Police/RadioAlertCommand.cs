using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class RadioAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_alert_radio"; }
        }

        public string Parameters
        {
            get { return "%message%"; }
        }

        public string Description
        {
            get { return "Envía un mensaje con tu radio al resto de los policías."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Ingresa el mensaje a enviar.", 1);
                return;
            }

            List<Group> MyGang = PlusEnvironment.GetGame().GetGroupManager().GetGangsForUser(Session.GetHabbo().Id);
            bool Police = false;
            bool Medic = false;
            if (MyGang == null || MyGang.Count <= 0)
            {
                Group Job = PlusEnvironment.GetGame().GetGroupManager().GetJob(Session.GetPlay().JobId);

                if (Job == null)
                {
                    Session.SendWhisper("¡No te encuentras trabajando en ningún empleo en el cual puedas usar el radio!", 1);
                    return;
                }

                if (Job.Id <= 0)
                {
                    Session.SendWhisper("¡No te encuentras trabajando en ningún empleo en el cual puedas usar el radio!", 1);
                    return;
                }

                Police = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "radio");
                Medic = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "radio_medic");

                if (!Police && !Medic && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                {
                    Session.SendWhisper("¡Tu empleo no requiere uso del Radio!", 1);
                    return;
                }
                
            }
            else
            {
                if (Session.GetPlay().IsWorking)
                    Medic = PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "radio_medic");
            }

            if (Session.GetPlay().DisableRadio)
            {
                Session.SendWhisper("¡Tu Radio está apagado! Escribe ':togglera' para encenderlo.", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);

            if (Session.GetHabbo().Translating)
            {
                string LG1 = Session.GetHabbo().FromLanguage.ToLower();
                string LG2 = Session.GetHabbo().ToLanguage.ToLower();

                PlusEnvironment.GetGame().GetClientManager().RadioAlert(PlusEnvironment.TranslateText(Message, LG1 + "|" + LG2) + " [" + LG1.ToUpper() + " -> " + LG2.ToUpper() + "]", Session, Police);
            }
            else
                PlusEnvironment.GetGame().GetClientManager().RadioAlert(Message, Session, Police, Medic);
        }
    }
}
