using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

namespace Plus.Communication.Packets.Outgoing.Sound
{
    class LoadJukeboxUserMusicItemsComposer : ServerPacket
    {
        public LoadJukeboxUserMusicItemsComposer(Room room)
            : base(ServerPacketHeader.LoadJukeboxUserMusicItemsMessageComposer)
        {
            var songs = room.GetTraxManager().GetAvaliableSongs();

            base.WriteInteger(songs.Count);//while

            foreach (var item in songs)
            {
                base.WriteInteger(item.Id);//item id
                base.WriteInteger(item.ExtradataInt);//Song id
            }
        }

        public LoadJukeboxUserMusicItemsComposer(ICollection<Item> Items)
            : base(ServerPacketHeader.LoadJukeboxUserMusicItemsMessageComposer)
        {

            base.WriteInteger(Items.Count);//while

            foreach (var item in Items)
            {
                base.WriteInteger(item.Id);//item id
                base.WriteInteger(item.ExtradataInt);//Song id
            }
        }
    }
}