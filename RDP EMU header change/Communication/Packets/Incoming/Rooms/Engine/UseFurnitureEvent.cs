using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.Communication.Packets.Outgoing.Rooms.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;

using Plus.Database.Interfaces;



namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    class UseFurnitureEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            if (Session.GetPlay().DrivingCar || Session.GetPlay().Pasajero || Session.GetHabbo().EscortID > 0 || Session.GetHabbo().Escorting > 0 || Session.GetHabbo().TaxiChofer > 0)
                return;

            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            int itemID = Packet.PopInt();
            Item Item = Room.GetRoomItemHandler().GetItem(itemID);
            if (Item == null)
                return;

            bool hasRights = false;
            if (Room.CheckRights(Session, false, true))
                hasRights = true;

            // New Poner items en área Terreno
            bool MyTerrain = false;
            if (Room.CheckTerrain(Session, Item.GetX, Item.GetY))
                MyTerrain = true;

            if (Item.GetBaseItem().InteractionType == InteractionType.banzaitele)
                return;

            if (Item.GetBaseItem().InteractionType == InteractionType.TELEPORT)
            {
                if (Session.GetPlay().DrivingCar || Session.GetPlay().Pasajero)
                {
                    Session.SendWhisper("No puedes entrar a establecimientos estando en un vehículo a la vez.", 1);
                    if (Item.GetX == Session.GetRoomUser().X && Item.GetY == Session.GetRoomUser().Y)
                        Session.GetRoomUser().MoveTo(Item.SquareInFront);
                    return;
                }
            }

            if (Item.GetBaseItem().InteractionType == InteractionType.TONER)
            {
                if (!Room.CheckRights(Session, true))
                    return;
                if (Room.TonerData.Enabled == 0)
                    Room.TonerData.Enabled = 1;
                else
                    Room.TonerData.Enabled = 0;

                Room.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));

                Item.UpdateState();

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `room_items_toner` SET `enabled` = '" + Room.TonerData.Enabled + "' LIMIT 1");
                }
                return;
            }

            if (Item.Data.InteractionType == InteractionType.GNOME_BOX && Item.UserID == Session.GetHabbo().Id)
            {
                Session.SendMessage(new GnomeBoxComposer(Item.Id));
            }

            Boolean Toggle = true;
            if (Item.GetBaseItem().InteractionType == InteractionType.WF_FLOOR_SWITCH_1 || Item.GetBaseItem().InteractionType == InteractionType.WF_FLOOR_SWITCH_2)
            {
                RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User == null)
                    return;

                if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    Toggle = false;
                }
            }

            string oldData = Item.ExtraData;
            int request = Packet.PopInt();

            Item.Interactor.OnTrigger(Session, Item, request, (hasRights || MyTerrain));

            if (Toggle)
                Item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, Session.GetHabbo(), Item);

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.EXPLORE_FIND_ITEM, Item.GetBaseItem().Id);
      
        }
    }
}
