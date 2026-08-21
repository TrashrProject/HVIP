using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    class ClubGiftsComposer : ServerPacket
    {
        public ClubGiftsComposer() 
            : base(ServerPacketHeader.ClubGiftsMessageComposer)
        {
            base.WriteInteger(-1);//Days until next gift.
            base.WriteInteger(0);//Gifts available
            // Exactly one gift entry is serialized below. Advertising a larger
            // count makes Nitro attempt to parse entries that are not present
            // and can trigger a DataView out-of-bounds RangeError.
            base.WriteInteger(1);//Count
            {
                base.WriteInteger(12701);
                base.WriteString("hc16_1");
                base.WriteBoolean(false);
                base.WriteInteger(1);
                base.WriteInteger(0);
                base.WriteInteger(0);
                base.WriteBoolean(true);
                base.WriteInteger(1);//Count for some reason
                {
                    base.WriteString("s");
                    base.WriteInteger(8228);
                    base.WriteString("");
                    base.WriteInteger(1);
                    base.WriteBoolean(false);
                }
              //  base.WriteInteger(0);
                //base.WriteBoolean(true);
            }

            // One trailing entry is serialized below, so the advertised count
            // must be 1. Advertising 0 while still writing the entry desynchronizes
            // Nitro's parser and can surface as a DataView out-of-bounds error.
            base.WriteInteger(1);//Count
            {
                //int, bool, int, bool
                base.WriteInteger(3253248);//Maybe the item id?

                base.WriteBoolean(false);//Can we get?
                base.WriteInteger(256);//idk
                base.WriteBoolean(false);//idk
                base.WriteInteger(0);
                base.WriteBoolean(true);//idk

            }
        }
    }
}
