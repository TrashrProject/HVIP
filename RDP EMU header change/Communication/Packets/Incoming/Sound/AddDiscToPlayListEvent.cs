using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Sound
{
    class AddDiscToPlayListEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var room = Session.GetHabbo().CurrentRoom;
            if (!room.CheckRights(Session))
                return;
            //Console.WriteLine(Packet.ToString());

            var itemid = Packet.PopInt();//item id
            var songid = Packet.PopInt();//Song id

            var item = room.GetRoomItemHandler().GetItem(itemid);
            if (item == null)
                return;
            if (!room.GetTraxManager().AddDisc(item))
                Session.SendMessage(new RoomNotificationComposer("", "¡Oops! ¡No ha sido posible poner el disco", "error", "", ""));

        }
    }
}
