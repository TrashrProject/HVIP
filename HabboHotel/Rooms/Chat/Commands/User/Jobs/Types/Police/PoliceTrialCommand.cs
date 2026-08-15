using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class PoliceTrialCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_trial"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Permite que una persona sea llevada a juicio."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }

            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
            {
                Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                return;
            }
            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "trial") && !Session.GetHabbo().GetPermissions().HasRight("give_police_trial"))
            {
                Session.SendWhisper("¡Solo un Jefe de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("give_police_trial"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }

            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes hacer eso con un usuario que está ausente!", 1);
                return;
            }
            // Sala de la corte de la ciudad.
            int PoliceTrialRoom = 4;
            int PoliceTrialRoom2 = 5;

            if (Room.Id != PoliceTrialRoom && Room.Id != PoliceTrialRoom2)
            {
                Session.SendWhisper("Lo sentimos, pero debes estar dentro de la sala de interrogatorio de la policía.", 1);
                return;
            }
            #endregion

            #region Execute
            if (TargetClient.GetPlay().PoliceTrial)
            {
                TargetClient.GetPlay().PoliceTrial = false;
                RoleplayManager.Shout(Session, "*Retira a " + TargetClient.GetHabbo().Username + " del Juicio Policial*", 37);
            }
            else
            {
                TargetClient.GetPlay().PoliceTrial = true;
                RoleplayManager.Shout(Session, "*Establece a " + TargetClient.GetHabbo().Username + " en un Juicio Policial*", 37);
            }
            #endregion
        }
    }
}