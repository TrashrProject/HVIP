using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;



namespace Plus.Communication.Packets.Incoming.Inventory.Furni
{
    class RequestFurniInventoryEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
            ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

            if (Session.GetHabbo().InventoryAlert == false)
            {
                Session.GetHabbo().InventoryAlert = true;
                int TotalCount = FloorItems.Count + WallItems.Count;
                if (TotalCount >= 2750)
                {
                    Session.SendNotification("Has superado el Máximo de funis en el inventario. Solo se te Mostrarán 2750 furnis de los " + TotalCount + " que tienes , si quieres ver los restantes, coloca algunos furnis en tus salas.");
                }
            }

            Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems));
        }
    }
}