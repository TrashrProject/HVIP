using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    class LookToEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (User.IsAsleep)
                return;
            
            User.UnIdle();

            if (User.GetClient().GetPlay().IsEscorted || User.GetClient().GetPlay().EscortingWalk || User.GetClient().GetHabbo().TaxiChofer > 0)
                return;

            int X = Packet.PopInt();
            int Y = Packet.PopInt();

            if ((X == User.X && Y == User.Y) || User.IsWalking || User.RidingHorse)
                return;

            int Rot = Rotation.Calculate(User.X, User.Y, X, Y);

            User.SetRot(Rot, false);
            User.UpdateNeeded = true;

            if (User.RidingHorse)
            {
                RoomUser Horse = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByVirtualId(User.HorseID);
                if (Horse != null)
                {
                    Horse.SetRot(Rot, false);
                    Horse.UpdateNeeded = true;
                }
            }

            /*
            if (User.GetClient().GetPlay().Chofer)
            {
                //Vars
                string Pasajeros = User.GetClient().GetPlay().Pasajeros;
                string[] stringSeparators = new string[] { ";" };
                string[] result;
                result = Pasajeros.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string psjs in result)
                {
                    GameClient PJ = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(psjs);
                    if (PJ != null)
                    {
                        PJ.GetRoomUser().SetRot(Rot, false);
                        PJ.GetRoomUser().UpdateNeeded = true;
                    }
                }
            }
            */

            // NEW RP by Jeihden
            /*
            if (User != null)
            {
                if (User.GetClient() != null)
                {
                    if (User.GetClient().GetPlay() != null)
                    {
                        User.GetClient().GetHabbo().HomeRoom = User.RoomId;
                        User.GetClient().GetPlay().LastCoordinates = User.X + "," + User.Y + "," + User.Z + "," + Rot;
                    }
                }
            }
            */
        }
    }
}
