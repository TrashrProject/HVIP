using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Notifications
{
    class NuxAlertMessageComposer : ServerPacket
    {
        public NuxAlertMessageComposer(string linkUrl) :
            base(ServerPacketHeader.NuxAlertMessageComposer)
        {
            base.WriteString(linkUrl);
        }
    }
}
