using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.HabboHotel.Users;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class HHHCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hhh"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Establece la vida, higiene y hambre de un usuario a su máximo valor."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length != 2)
            {
                Session.SendWhisper("Comando inválido, escribe ':hhh [usuario]'", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }
            #endregion

            #region Execute
            TargetClient.GetPlay().CurHealth = TargetClient.GetPlay().MaxHealth;
            TargetClient.GetPlay().Hunger = 0;
            TargetClient.GetPlay().Hygiene = 100;

            Session.SendWhisper("((Has establecido los Stats básicos de " + TargetClient.GetHabbo().Username + " al Máximo))", 1);

            TargetClient.SendWhisper("((Te han establecido tus Stats básicos al Máximo))", 1);
            // Refrescamos WS
            TargetClient.GetPlay().UpdateInteractingUserDialogues();
            TargetClient.GetPlay().RefreshStatDialogue();
            #endregion
        }
    }
}