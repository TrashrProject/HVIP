using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using System.Globalization;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class HideWiredCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hidewired"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Oculta los Wired de la sala."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (!Room.CheckRights(Session, false, false))
            {
                Session.SendWhisper("¡No tienes permiso para ocultar Wireds en esta zona!", 1);
                return;
            }

            Room.HideWired = !Room.HideWired;
            if (Room.HideWired)
                Session.SendWhisper("Los Wired han sido ocultados.", 1);
            else
                Session.SendWhisper("Los Wired ahora son visibles.", 1);

            using (IQueryAdapter con = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                con.SetQuery("UPDATE `rooms` SET `hide_wired` = @enum WHERE `id` = @id LIMIT 1");
                con.AddParameter("enum", PlusEnvironment.BoolToEnum(Room.HideWired));
                con.AddParameter("id", Room.Id);
                con.RunQuery();
            }

            List<ServerPacket> list = new List<ServerPacket>();

            list = Room.HideWiredMessages(Room.HideWired);

            Room.SendMessage(list);
        }
    }
}