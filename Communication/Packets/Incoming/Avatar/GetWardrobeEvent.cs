/*using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Avatar;

namespace Plus.Communication.Packets.Incoming.Avatar
{
    class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new WardrobeComposer(Session));
        }
    }
}
*/
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Avatar;
using Plus.HabboHotel.RolePlay.PlayRoom;

namespace Plus.Communication.Packets.Incoming.Avatar
{
    class GetWardrobeEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetRoomUser() == null)
                return;

            //Generamos la Sala
            Room Room = RoleplayManager.GenerateRoom(Session.GetRoomUser().RoomId);

            if (Session.GetRoomUser() == null || !Session.GetHabbo().InRoom)
            {
                Session.SendNotification("Debes dirigirte a una tienda de ropa o a un probador para cambiar tu atuendo.");
                return;
            }

            if (!Room.WardrobeEnabled)
            {
                Session.SendNotification("Debes dirigirte a una tienda de ropa o a un probador para cambiar tu atuendo.");
                return;
            }

            Session.SendMessage(new WardrobeComposer(Session));
        }
    }
}
