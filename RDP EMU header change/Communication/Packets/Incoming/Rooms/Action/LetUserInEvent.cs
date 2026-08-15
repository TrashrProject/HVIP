using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    class LetUserInEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session))
                return;

            string Name = Packet.PopString();
            bool Accepted = Packet.PopBoolean();

            GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Name);
            if (Client == null)
                return;

            if (Accepted)
            {
                RoleplayManager.Shout(Client, "*Entra a un apartamento*", 5);

                Client.GetHabbo().LetInAppartment = true;
                Client.SendMessage(new FlatAccessibleComposer(""));
                Room.SendMessage(new FlatAccessibleComposer(Client.GetHabbo().Username), true);
                Client.GetHabbo().PrepareRoom(Room.Id, "");
            }
            else
            {
                //Client.SendMessage(new FlatAccessDeniedComposer(""));
                Room.SendMessage(new FlatAccessDeniedComposer(Client.GetHabbo().Username), true);
                Client.SendMessage(new RoomNotificationComposer("builders_club_room_locked_small", 3, "Han rechazado tu entrada al apartamento.", ""));
            }
        }
    }
}
