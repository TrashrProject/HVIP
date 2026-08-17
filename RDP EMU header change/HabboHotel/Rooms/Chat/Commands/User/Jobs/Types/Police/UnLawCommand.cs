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
    class UnLawCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_law_undo"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Quita a una persona de la lista de buscados."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            Habbo Target = PlusEnvironment.GetHabboByUsername(Params[1]);

            if (Target == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            var RoomUser = Session.GetRoomUser();

            if (RoomUser == null)
                return;


            if (!PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Session, "unlaw") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de Policía puede hacer eso!", 1);
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (Target.Id == Session.GetHabbo().Id)
                {
                    Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                    return;
                }
                if (PlusEnvironment.GetGame().GetGroupManager().HasJobCommand(Target.GetClient(), "law"))
                {
                    Session.SendWhisper("¡No puedes hacer eso entre compañeros de trabajo!", 1);
                    return;
                }
            }
            if (!Session.GetPlay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            if (!RoleplayManager.WantedList.ContainsKey(Target.Id))
            {
                Session.SendWhisper("¡Esa persona no está siendo buscada!", 1);
                return;
            }

            if (Target.GetClient() != null && Target.GetClient().GetRoomUser() != null)
            {
                if (Target.GetClient().GetRoomUser().IsAsleep)
                {
                    Session.SendWhisper("¡No puedes quitar de la Lista a un usuario ausente!", 1);
                    return;
                }
            }
            #endregion

            #region Execute
            Wanted Junk;
            RoleplayManager.WantedList.TryRemove(Target.Id, out Junk);
            RoleplayManager.Shout(Session, "*Retira los cargos de " + Target.Username + " y es removido de la Lista de Buscados*", 37);

            if (Target.GetClient() != null)
                Target.GetClient().GetPlay().IsWanted = false;// El timer detecta False y se destruye el timer.
            #endregion
        }
    }
}