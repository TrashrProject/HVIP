using Plus.Communication.Packets.Outgoing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plus.Communication.Packets.Incoming.Alfas
{
    class InviteToRoom : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var requester = Session.GetHabbo().GuideOtherUser;
            var room = Session.GetHabbo().CurrentRoom;
            var message = new ServerPacket(ServerPacketHeader.OnGuideSessionInvitedToGuideRoomMessageComposer);
            //onGuideSessionInvitedToGuideRoom
            if (room == null)
            {
                message.WriteInteger(0); //id de l'appart
                message.WriteString("");
            }
            else
            {
                message.WriteInteger(room.RoomId); //id de l'appart
                message.WriteString(room.RoomData.Name);
            }
            requester.SendMessage(message);
            Session.SendMessage(message);
        }
    }
}
