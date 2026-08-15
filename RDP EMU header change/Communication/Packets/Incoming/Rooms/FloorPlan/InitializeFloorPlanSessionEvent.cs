using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboRoleplay.ApartmentsOwned;

namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan
{
    class InitializeFloorPlanSessionEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetRoomUser() == null)
                return;

            ApartmentOwned AP = PlusEnvironment.GetGame().GetApartmentOwnedManager().GetApartmentOwnedById(Session.GetRoomUser().RoomId);

            if (AP != null && !AP.FloorEditor)
            {
                Session.SendNotification("¡Este apartamento no tiene uso de Floor Editor!");
                return;
            }
        }
    }
}
