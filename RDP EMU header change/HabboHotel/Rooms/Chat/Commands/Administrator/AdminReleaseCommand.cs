using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrators
{
    class AdminReleaseCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_release"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Libera a una persona encarcelada."; }
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

            var RoomUser = Session.GetRoomUser();
            var TargetRoomUser = TargetClient.GetRoomUser();

            if (RoomUser == null || TargetRoomUser == null)
                return;

            if (!TargetClient.GetPlay().IsJailed)
            {
                Session.SendWhisper("No puedes liberar a una persona que no está encarcelada.", 1);
                return;
            }
            #endregion

            #region Execute
            RoleplayManager.Shout(Session, "*Libera a " + TargetClient.GetHabbo().Username + " de su condena en Prisión.*", 23);
            TargetClient.GetPlay().IsJailed = false;
            TargetClient.GetPlay().JailedTimeLeft = 0;
            TargetClient.GetHabbo().Poof(true);

            #endregion
        }
    }
}