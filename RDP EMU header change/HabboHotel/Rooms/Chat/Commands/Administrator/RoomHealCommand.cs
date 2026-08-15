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
    class RoomHealCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room_heal"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Cura a todas las personas del mismo sitio que tú."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Execute
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

                GameClient TargetClient = User.GetClient();

                if (TargetClient.GetRoomUser() != null)
                    TargetClient.GetRoomUser().ApplyEffect(0);

                if (TargetClient.GetPlay().IsDead || TargetClient.GetPlay().IsDying)
                    continue;

                TargetClient.GetPlay().ReplenishStats();
                TargetClient.SendWhisper("¡Un administrador los ha curado a todos!", 1);

                // Refrescamos WS
                TargetClient.GetPlay().UpdateInteractingUserDialogues();
                TargetClient.GetPlay().RefreshStatDialogue();
            }

            RoleplayManager.Shout(Session, "*Comienza a curar las heridas de todas las personas en esta zona*", 23);
            return;
            #endregion
        }
    }
}