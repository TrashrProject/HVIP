using System;
using System.Threading;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.HabboRoleplay.Houses;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Apartment
{
    class SetPriceommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_house_set_price"; }
        }

        public string Parameters
        {
            get { return "%amount%"; }
        }

        public string Description
        {
            get { return "Establece el precio de venta de tu casa a la cantidad deseada."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            List<House> House = PlusEnvironment.GetGame().GetHouseManager().GetHouseByOwnerId(Session.GetHabbo().Id);

            if (House == null || House.Count <= 0)
            {
                Session.SendWhisper("No tienes ninguna casa a tu nombre para vender.", 1);
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el precio de venta de tu casa.", 1);
                return;
            }

            int Price;
            if (int.TryParse(Params[1], out Price))
            {
                House[0].UpdateCost(Price);
                Session.SendNotification("Has colocado tu casa en venta por $" + String.Format("{0:N0}", Price));
                return;
            }
            else
            {
                Session.SendWhisper("Debes ingresar el precio de venta de tu casa.", 1);
                return;
            }
        }
    }
}
