using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Notifications
{
    class SuperNotificationComposer : ServerPacket
    {
        public SuperNotificationComposer(string image, string title, string message, string linkTitle, string linkUrl) :
            base(ServerPacketHeader.SuperNotificationMessageComposer)
        {
            base.WriteString(image);
            base.WriteInteger((linkTitle != string.Empty && linkUrl != string.Empty) ? 4 : 2);
            base.WriteString("title");
            base.WriteString(title);
            base.WriteString("message");
            base.WriteString(message);
            base.WriteString("linkTitle");
            base.WriteString(linkTitle);
            base.WriteString("linkUrl");
            base.WriteString(linkUrl);
        }
    }
}