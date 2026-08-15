using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Avatar;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorWardrobe : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.Coordinate.X, Item.Coordinate.Y, User.Coordinate.X, User.Coordinate.Y))
            {
                if (User.CanWalk)
                    User.MoveTo(Item.Coordinate);
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }

    }
}