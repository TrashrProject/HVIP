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
    class RoomReleaseCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_admin_room_release"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Libera de prisión a todas las personas."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            int JailedUsers = 0;
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

                if (!User.GetClient().GetPlay().IsJailed)
                    continue;

                JailedUsers++;
            }

            if (JailedUsers <= 0)
            {
                Session.SendWhisper("¡No hay nadie encarcelado aquí!", 1);
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

                    if (!User.GetClient().GetPlay().IsJailed)
                        continue;

                    GameClient TargetClient = User.GetClient();

                    TargetClient.GetPlay().IsJailed = false;
                    TargetClient.GetPlay().JailedTimeLeft = 0;
                    TargetClient.GetHabbo().Poof(true);
                    TargetClient.SendWhisper("¡Un administrador los ha liberado a todos de Prisión!", 1);
                }
                RoleplayManager.Shout(Session, "*Libera a todos de su condena en Prisión.*", 23);
                return;
            }
            #endregion
        }
    }
}