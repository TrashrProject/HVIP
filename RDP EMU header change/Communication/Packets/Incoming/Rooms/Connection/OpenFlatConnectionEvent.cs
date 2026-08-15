using System;

using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Connection
{
    public class OpenFlatConnectionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null)
                return;

            int RoomId = Packet.PopInt();
            string Password = Packet.PopString();

            // new
            if(RoomId <= 0)
            {
                return;
                //RoomId = Session.GetHabbo().HomeRoom;
                //Password = null;
            }
        
            Session.GetHabbo().PrepareRoom(RoomId, Password);
        }
    }
}