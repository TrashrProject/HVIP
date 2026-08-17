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
    class RoomRestoreCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_room_restore"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Da de alta a todas las personas muertas del Hospital."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int DeadUsers = 0;
            #endregion

            #region Conditions
            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (User == null)
                    continue;

                if (User.IsBot)
                    continue;

                if (User.GetClient() == null)
                    continue;

                if (User.GetClient().GetPlay() == null)
                    continue;

                if (!User.GetClient().GetPlay().IsDead && !User.GetClient().GetPlay().IsDying)
                    continue;

                DeadUsers++;
            }

            if (DeadUsers <= 0)
            {
                Session.SendWhisper("¡No hay nadie muert@!", 1);
                return;
            }
            #endregion

            #region Execute
            else
            {
                foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (User == null)
                        continue;

                    if (User.IsBot)
                        continue;

                    if (User.GetClient() == null)
                        continue;

                    if (User.GetClient().GetPlay() == null)
                        continue;

                    if (!User.GetClient().GetPlay().IsDead && !User.GetClient().GetPlay().IsDying)
                        continue;

                    GameClient TargetClient = User.GetClient();

                    TargetClient.GetPlay().IsDead = false;
                    TargetClient.GetPlay().DeadTimeLeft = 0;
                    TargetClient.GetPlay().IsDying = false;
                    TargetClient.GetPlay().DyingTimeLeft = 0;
                    TargetClient.GetPlay().ReplenishStats(true);
                    TargetClient.GetRoomUser().CanWalk = true;
                    TargetClient.GetRoomUser().Frozen = false;
                    TargetClient.SendWhisper("Un administrado los ha dado de alta a todos.", 1);
                    // Refrescamos WS
                    TargetClient.GetPlay().UpdateInteractingUserDialogues();
                    TargetClient.GetPlay().RefreshStatDialogue();
                }

                RoleplayManager.Shout(Session, "*Atiende y revive a todas las personas de la Zona*", 5);
                return;
            }
            #endregion
        }
    }
}