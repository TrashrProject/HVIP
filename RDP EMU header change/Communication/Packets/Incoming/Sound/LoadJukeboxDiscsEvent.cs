using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Sound
{
    class LoadJukeboxDiscsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session.GetHabbo().CurrentRoom != null)
                Session.SendMessage(new LoadJukeboxUserMusicItemsComposer(Session.GetHabbo().CurrentRoom));
        }
    }
}
