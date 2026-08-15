namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using Plus;
    using Plus.Communication.Packets.Outgoing.Inventory.Purse;
    using Plus.Database.Interfaces;
    using Plus.HabboHotel.GameClients;
    using Plus.HabboHotel.Rooms;
    using Plus.HabboHotel.Rooms.Chat.Commands;
    using Plus.HabboHotel.Users;
    using System;

    internal class BuyRoomCommand : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            Room currentRoom = Session.GetHabbo().CurrentRoom;
            RoomUser roomUserByHabbo = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(currentRoom.OwnerName);
            if (currentRoom != null)
            {
                if (currentRoom.OwnerName == Session.GetHabbo().Username)
                {
                    Session.SendNotification("Ya tienes esta Sala!");
                }
                else if (!Room.ForSale)
                {
                    Session.SendNotification("\x00a1Esta Sala no esta en Venta!");
                }
                else if (Session.GetHabbo().Duckets < currentRoom.SalePrice)
                {
                    Session.SendNotification("\x00a1No tiene suficientes Duckets para comprar esta Sala!");
                }
                else if ((roomUserByHabbo == null) || (roomUserByHabbo.GetClient() == null))
                {
                    Session.SendNotification("Se ha Producido un error. Esta sala no esta en Venta");
                    currentRoom.ForSale = false;
                    currentRoom.SalePrice = 0;
                }
                else
                {
                    GameClient client = roomUserByHabbo.GetClient();
                    Habbo habbo = client.GetHabbo();
                    habbo.Duckets += currentRoom.SalePrice;
                    client.SendMessage(new HabboActivityPointNotificationComposer(client.GetHabbo().Duckets, currentRoom.SalePrice, 0));
                    Habbo habbo2 = Session.GetHabbo();
                    habbo2.Duckets -= currentRoom.SalePrice;
                    Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets, currentRoom.SalePrice, 0));
                    currentRoom.OwnerName = Session.GetHabbo().Username;
                    currentRoom.OwnerId = Session.GetHabbo().Id;
                    currentRoom.RoomData.OwnerName = Session.GetHabbo().Username;
                    currentRoom.RoomData.OwnerId = Session.GetHabbo().Id;
                    int roomId = currentRoom.RoomId;
                    using (IQueryAdapter adapter = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        adapter.RunQuery(string.Concat(new object[] { "UPDATE rooms SET owner='", Session.GetHabbo().Id, "' WHERE id='", Room.RoomId, "' LIMIT 1" }));
                        adapter.RunQuery(string.Concat(new object[] { "UPDATE items SET user_id='", Session.GetHabbo().Id, "' WHERE room_id='", Room.RoomId, "'" }));
                    }
                    Session.GetHabbo().UsersRooms.Add(currentRoom.RoomData);
                    client.GetHabbo().UsersRooms.Remove(currentRoom.RoomData);
                    PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(currentRoom, false);
                    RoomData data = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(roomId);
                    Session.GetHabbo().PrepareRoom(Session.GetHabbo().CurrentRoom.RoomId, "");
                }
            }
        }

        public string Description =>
            "Compra una sala en venta de cualquier usuario";

        public string Parameters =>
            "";

        public string PermissionRequired =>
            "command_buy_room";
    }
}

