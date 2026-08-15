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
    class NewsReporterToggle : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_newsreporter"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Le da o quita permisos de Reporter@ a un usuario."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length != 2)
            {
                Session.SendWhisper("Comando inválido, escribe ':reporter [usuario]'", 1);
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
            TargetClient.GetPlay().IsNewsReporter = !TargetClient.GetPlay().IsNewsReporter;
            RoleplayManager.SaveQuickStat(TargetClient, "is_news_reporter", TargetClient.GetPlay().IsNewsReporter ? "1" : "0");

            if (TargetClient.GetPlay().IsNewsReporter)
            {
                RoleplayManager.Shout(Session, "*Nombra a " + TargetClient.GetHabbo().Username + " como reporter@*", 5);
                TargetClient.SendNotification("Te han dado permisos para crear y editar noticias y eventos. Reincia la página para cargar tu nuevo panel de control.");
			}
			else
			{
                RoleplayManager.Shout(Session, "*Le retira a " + TargetClient.GetHabbo().Username + " el cargo de reporter@*", 5);
                TargetClient.SendNotification("Te han retirado los permisos de reporter@");
                TargetClient.GetPlay().RefreshStatDialogue();
            }
            #endregion
        }
    }
}