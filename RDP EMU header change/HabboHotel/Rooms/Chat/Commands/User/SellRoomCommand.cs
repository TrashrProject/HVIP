namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    using Plus.HabboHotel.GameClients;
    using Plus.HabboHotel.Rooms;
    using Plus.HabboHotel.Rooms.Chat.Commands;
    using System;

    internal class SellRoomCommand : IChatCommand
    {
        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Room.CheckRights(Session, true, false))
            {
                if (Room == null)
                {
                    if (Params.Length == 1)
                    {
                        Session.SendWhisper("Oops, Se olvido de elegir un precio para vender esta sala.", 0);
                        return;
                    }
                    if (Room.Group != null)
                    {
                        Session.SendWhisper("Oops, al parecer esta sala tiene un grupo, asi no se podra vender, primero debe eliminar el grupo.", 0);
                        return;
                    }
                }
                int result = 0;
                if (!int.TryParse(Params[1], out result))
                {
                    Session.SendWhisper("Oops, estas introduciendo un valor que no es correcto", 0);
                }
                else if (result < 0)
                {
                    Session.SendWhisper("No puede vender una sala con un valor numerico Negativo.", 0);
                }
                else
                {
                    if (Room.ForSale)
                    {
                        Room.SalePrice = result;
                    }
                    else
                    {
                        Room.ForSale = true;
                        Room.SalePrice = result;
                    }
                    foreach (RoomUser user in Room.GetRoomUserManager().GetRoomUsers())
                    {
                        if ((user != null) && (user.GetClient() != null))
                        {
                            Session.SendWhisper("Esta sala esta en venta, su Precio actual es  " + result + " Duckets! Comprala escribiendo :buyroom", 0);
                        }
                    }
                    Session.SendNotification("Si usted quiere vender su sala, debe incluir un valor numerico. \n\nPOR FAVOR NOTA:\nSi usted vende una sala, no la puede Recuperar de nuevo.!\n\nUsted puede cancelar la venta de una habitaci\x00f3n escribiendo ':unload' (sin las '')");
                }
            }
        }

        public string Description =>
            "Pon a la venta tu sala con todos los furnis adentro";

        public string Parameters =>
            "%Value%";

        public string PermissionRequired =>
            "command_sell_room";
    }
}

