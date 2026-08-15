using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class Call911Command : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_911"; }
        }

        public string Parameters
        {
            get { return "%message%"; }
        }

        public string Description
        {
            get { return "Llama a la policía"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (Session.GetPlay().TryGetCooldown("call911"))
                return;

            if (RoleplayManager.PurgeEvent)
            {
                Session.SendWhisper("¡Ese servicio no está disponible durante la purga!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);

            RoleplayManager.Shout(Session, "*Llama al 911 por ayuda*", 5);

            if (string.IsNullOrEmpty(Message))
                PlusEnvironment.GetGame().GetClientManager().JailAlert(Session.GetHabbo().Username + " ha llamado al 911 en " + Room.Name + " ¡Ve allí rápidamente!");
            else
                PlusEnvironment.GetGame().GetClientManager().JailAlert(Session.GetHabbo().Username + " ha llamado al 911 en " + Room.Name + " con el mensaje: " + Message);

            Session.GetPlay().CooldownManager.CreateCooldown("call911", 1000, 30);
        }
    }
}
