using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Outgoing.Rooms.Chat
{
    public class WhisperComposer : ServerPacket
    {
        public WhisperComposer(int VirtualId, string Message, int Emotion, int Colour)
            : base(ServerPacketHeader.WhisperMessageComposer)
        {
            base.WriteInteger(VirtualId);
            base.WriteString(Message);
            base.WriteInteger(Emotion);
            base.WriteInteger(Colour);
            base.WriteInteger(0);
            base.WriteInteger(-1);
        }
    }
}