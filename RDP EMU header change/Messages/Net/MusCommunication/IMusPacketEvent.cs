using Plus.HabboHotel.GameClients;

namespace Plus.Messages.Net.MusCommunication
{
    public interface IMusPacketEvent
    {
        void Parse(MusConnection MUS, MusPacketEvent Packet);
    }
}