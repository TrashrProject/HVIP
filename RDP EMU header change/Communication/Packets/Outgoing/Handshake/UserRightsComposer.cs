using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Handshake
{
    public class UserRightsComposer : ServerPacket
    {
        public UserRightsComposer(int Rank, GameClient session)
            : base(ServerPacketHeader.UserRightsMessageComposer)
        {
            base.WriteInteger(2);//Club level
            base.WriteInteger(Rank);
            base.WriteBoolean(session.GetHabbo().GetPermissions().HasRight("ambassador_tool"));//Is an ambassador
        }
    }
}