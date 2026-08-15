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
    class RestoreCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_restore"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Da de alta a una persona muerta en el Hospital."; }
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
            if (!TargetClient.GetPlay().IsDead && !TargetClient.GetPlay().IsDying)
            {
                Session.SendWhisper("¡Esa persona no se encuentra herida!", 1);
                return;
            }
            var RoomUser = Session.GetRoomUser();
            var TargetRoomUser = TargetClient.GetRoomUser();

            if (RoomUser == null || TargetRoomUser == null)
                return;
            #endregion

            #region Execute

            RoleplayManager.Shout(Session, "*Da de alta a " + TargetClient.GetHabbo().Username + ", curandol@*", 23);
            TargetClient.GetPlay().IsDead = false;
            TargetClient.GetPlay().IsDying = false;
            TargetClient.GetRoomUser().ApplyEffect(0);
            TargetClient.GetPlay().DeadTimeLeft = 0;
            TargetClient.GetPlay().DyingTimeLeft = 0;
            TargetClient.GetPlay().CurHealth = TargetClient.GetPlay().MaxHealth;
            TargetClient.GetRoomUser().CanWalk = true;
            TargetClient.GetRoomUser().Frozen = false;
            TargetClient.SendWhisper("Un administrador te ha dado de alta.", 1);
            // Refrescamos WS
            TargetClient.GetPlay().UpdateInteractingUserDialogues();
            TargetClient.GetPlay().RefreshStatDialogue();
            #endregion
        }
    }
}