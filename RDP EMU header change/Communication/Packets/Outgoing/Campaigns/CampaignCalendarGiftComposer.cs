using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.LandingView
{
    class CampaignCalendarGiftComposer : ServerPacket
    {
        public CampaignCalendarGiftComposer(string iconName = "throne")
            : base(ServerPacketHeader.CampaignCalendarGiftMessageComposer)
        {
            base.WriteBoolean(true); // never bothered to check
            base.WriteString("xmas14_starfish"); //productName
            base.WriteString(""); //customImage
            base.WriteString(iconName); //iconName
        }
    }
}
