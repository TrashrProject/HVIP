using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Campaigns
{
    class CampaignCalendarDataComposer : ServerPacket
    {
        public CampaignCalendarDataComposer(List<int> OpenedGifts)
            : base(ServerPacketHeader.CampaignCalendarDataMessageComposer)
        {
            int currentDate = DateTime.Now.Day - 1;

            base.WriteString("xmas16"); //eventTrigger
            base.WriteString(string.Empty); //idk? same as habbo ;P
            base.WriteInteger(currentDate); //currentDate
            base.WriteInteger(500); //totalAmountOfDays

            base.WriteInteger(OpenedGifts.Count); //countOpenGifts
            foreach (int Opened in OpenedGifts)
            {
                base.WriteInteger(Opened); //giftDay
            }

            List<int> MissedGifts = Enumerable.Range(0, (currentDate - 2)).Where(Day => !OpenedGifts.Contains(Day)).ToList();

            base.WriteInteger(MissedGifts.Count); //countMissedGifts
            foreach (int Missed in MissedGifts)
            {
                base.WriteInteger(Missed); //giftDay
            }
        }
    }
}