using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GiveRoom : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_give_room"; }
        }

        public string Parameters
        {
            get { return "%cantidad%"; }
        }

        public string Description
        {
            get { return "Da dinero a todos de tu zona actual."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar la cantidad de dinero a dar.", 1);
                return;
            }
            int Amount;
            if (int.TryParse(Params[1], out Amount))
            {
                foreach (RoomUser RoomUser in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (RoomUser == null || RoomUser.GetClient() == null || Session.GetHabbo().Id == RoomUser.UserId)
                        continue;
                    RoomUser.GetClient().GetHabbo().Credits += Amount;
                    RoomUser.GetClient().GetPlay().MoneyEarned += Amount;
                    RoomUser.GetClient().SendMessage(new CreditBalanceComposer(RoomUser.GetClient().GetHabbo().Credits));
                }
                Session.SendWhisper("Dinero entregado a todos en la zona.", 1);
            }
            else
            {
                Session.SendWhisper("Cantidad inválida.", 1);
                return;
            }
        }
    }
}
  