using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.RolePlay.PlayRoom;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ToggleChNCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_chanel"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Activa o desactiva el Canal :n."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            Session.GetPlay().ChNDisabled = !Session.GetPlay().ChNDisabled;
            Session.SendWhisper("Ahora " + (Session.GetPlay().ChNDisabled == true ? "no puedes" : "puedes") + " ver y usar el Canal :n.", 1);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `play_stats` SET `chn_disabled` = @ChNDisabled WHERE `id` = '" + Session.GetHabbo().Id + "'");
                dbClient.AddParameter("ChNDisabled", PlusEnvironment.BoolToEnum(Session.GetPlay().ChNDisabled));
                dbClient.RunQuery();
            }
        }
    }
}