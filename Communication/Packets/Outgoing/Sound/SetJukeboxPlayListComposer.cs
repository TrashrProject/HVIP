using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.TraxMachine;

namespace Plus.Communication.Packets.Outgoing.Sound
{
    class SetJukeboxPlayListComposer : ServerPacket
    {
        public SetJukeboxPlayListComposer(Room room)
            : base(ServerPacketHeader.SetJukeboxPlayListMessageComposer)
        {
            var items = room.GetTraxManager().Playlist;
            base.WriteInteger(items.Count); //Capacity
            base.WriteInteger(items.Count); //While items Songs Count

            foreach (var item in items)
            {
                int musicid;
                int.TryParse(item.ExtraData, out musicid);
                base.WriteInteger(item.Id);
                base.WriteInteger(musicid);//EndWhile
            }
        }
    }
}