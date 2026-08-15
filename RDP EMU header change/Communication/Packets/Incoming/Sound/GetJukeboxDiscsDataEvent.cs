using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Sound;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing;
using Plus.HabboHotel.Rooms.TraxMachine;

namespace Plus.Communication.Packets.Incoming.Sound
{
    class GetJukeboxDiscsDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var songslen = Packet.PopInt();
            var Songs = new List<TraxMusicData>();
            while (songslen-- > 0)
            {
                var id = Packet.PopInt();
                var music = TraxSoundManager.GetMusic(id);
                if (music != null)
                    Songs.Add(music);
            }
            if (Session.GetHabbo().CurrentRoom != null)
                Session.SendMessage(new SetJukeboxSongMusicDataComposer(Songs));
        }
    }
}
