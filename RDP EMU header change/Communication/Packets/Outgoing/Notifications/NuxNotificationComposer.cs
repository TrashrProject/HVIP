using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;


namespace Plus.Communication.Packets.Outgoing.Users
{
    class NuxNotificationComposer : ServerPacket
    {
        public NuxNotificationComposer(string Message)
            : base(ServerPacketHeader.NuxNotificationMessageComposer)
        {
            base.WriteInteger(3958885);
            base.WriteString(Message);
            base.WriteBoolean(false);
            base.WriteInteger(1013474668);
        }
    }
}
