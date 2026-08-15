using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class SmokeWeedCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_smokeweed"; }

        }

        public string Parameters
        {
            get { return ""; }

        }

        public string Description
        {
            get { return "Fuma un porro."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            Task.Run(async delegate
            {
                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*" + Session.GetHabbo().Username + ", enrollando un porro*", 0, ThisUser.LastBubble));
                await Task.Delay(1000);
                Session.GetHabbo().Effects().ApplyEffect(5);
                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*" + Session.GetHabbo().Username + ", encendiendo el porro*", 0, ThisUser.LastBubble));
                await Task.Delay(500);
                Session.GetHabbo().Effects().ApplyEffect(0);
                await Task.Delay(1000);
                Session.GetHabbo().Effects().ApplyEffect(53);
                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, "*" + Session.GetHabbo().Username + ", fumando la marihuana*", 0, ThisUser.LastBubble));
                await Task.Delay(5000);
                Session.GetHabbo().Effects().ApplyEffect(0);
            });
        }
    }
}