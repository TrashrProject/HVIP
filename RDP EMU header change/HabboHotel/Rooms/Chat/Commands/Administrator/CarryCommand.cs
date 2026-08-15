using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    class CarryCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_carry"; }
        }

        public string Parameters
        {
            get { return "%ItemId%"; }
        }

        public string Description
        {
            get { return "Aparece un handitem en tu personaje."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            int ItemId = 0;
            if (!int.TryParse(Convert.ToString(Params[1]), out ItemId))
            {
                Session.SendWhisper("Debes ingresar un ID válido de item.", 1);
                return;
            }

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            User.CarryItem(ItemId);
        }
    }
}
