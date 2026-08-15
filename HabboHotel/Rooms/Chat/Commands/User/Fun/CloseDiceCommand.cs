using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class CloseDiceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_close_dice"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Cierra los 5 dados cerca de tu personaje acomodados de la forma tradicional."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser roomUser = Room?.GetRoomUserManager()?.GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (roomUser == null)
            {
                return;
            }

            List<Items.Item> userBooth = Room.GetRoomItemHandler().GetFloor.Where(x => x != null && Gamemap.TilesTouching(
                x.Coordinate, roomUser.Coordinate) && x.Data.InteractionType == Items.InteractionType.DICE).ToList();

            if (userBooth.Count != 5)
            {
                Session.SendWhisper("Debes tener 5 dados cerca de ti.", 1);
                return;
            }

            userBooth.ForEach(x => {
                x.ExtraData = "0";
                x.UpdateState();
            });

            Session.SendWhisper("Los dados han sido cerrados.", 1);
        }
    }
}