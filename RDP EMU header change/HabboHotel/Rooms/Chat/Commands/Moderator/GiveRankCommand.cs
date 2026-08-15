using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GiveRankCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_giverank"; }
        }

        public string Parameters
        {
            get { return "%username% %rankid%"; }
        }

        public string Description
        {
            get { return "Da rango a una persona."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            // Special permissions for an Admin that cans give rank.
            int UserIdSpecialAdmin3 = 9447; // MegaDude

            if (Session.GetHabbo().Rank < 6 && (
                Session.GetHabbo().Id != UserIdSpecialAdmin3))
            {
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar el nombre de la persona y el Id del rango (1-5)", 1);
                return;
            }

            GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (Target == null)
            {
                Session.SendWhisper("No se pudo encontrar a esa persona.", 1);
                return;
            }

            if (Target.GetHabbo().Rank >= Session.GetHabbo().Rank)
			{
                Session.SendWhisper("No puedes modificar el rango de un superior o del mismo rango que tú.", 1);
                return;
            }

            if (!int.TryParse(Params[2], out int rankID) || rankID <= 0 || rankID >= 6)
			{
                Session.SendWhisper("Ingresa un Id de rango válido (1-5).", 1);
                return;
            }

            Target.GetHabbo().Rank = rankID;
            RoleplayManager.SaveQuickUserInfo(Target, "rank", rankID.ToString());
            Target.GetHabbo().GetPermissions().Init(Target.GetHabbo());

            Target.SendWhisper("Te han asignado el rango " + rankID + ". Tus permisos y comandos han sido actualizados.", 1);
            Session.SendWhisper("Le has asignado el rango " + rankID + " a " + Target.GetHabbo().Username + " correctamente.", 1);
        }
    }
}