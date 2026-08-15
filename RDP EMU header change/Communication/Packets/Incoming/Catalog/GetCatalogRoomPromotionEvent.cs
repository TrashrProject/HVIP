using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboRoleplay.Misc;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    class GetCatalogRoomPromotionEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new GetCatalogRoomPromotionComposer(Session.GetHabbo().UsersRooms));

            int OldRoom = Session.GetHabbo().HomeRoom;

            if (OldRoom <= 0)
                OldRoom = 1;

            RoleplayManager.SendUserOld(Session, OldRoom, "¡Vaya! Al parecer esta Zona ha sido recargada.\n\nIntentaremos enviarte de vuelta.\n\n((Si el problema persiste, puedes reiniciar el client))");

        }
    }
}
