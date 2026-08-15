using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ClearWantedCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_clear_wanted"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Limpia la lista de Buscados."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            int Bubble = 37;
            var RoomUser = Session.GetRoomUser();

            if (RoomUser == null)
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "clearwanted") && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
            {
                Session.SendWhisper("¡Solo un Jefe de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }

            if (RoleplayManager.WantedList.Count <= 0)
            {
                Session.SendWhisper("¡La lista ya se encuentra vacía!", 1);
                return;
            }

            if (Session.GetHabbo().GetPermissions().HasRight("roleplay_corp_manager") && !Session.GetPlay().IsWorking)
                Bubble = 24;
            #endregion

            #region Execute
            RoleplayManager.Shout(Session, "*Borra toda la lista de buscados, quitando a cualquiera que todavía estaba en ella*", Bubble);
            RoleplayManager.WantedList.Clear();
            #endregion
        }
    }
}