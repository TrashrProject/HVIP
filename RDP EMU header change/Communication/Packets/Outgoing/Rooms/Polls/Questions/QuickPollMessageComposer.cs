using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Notifications
{
    class QuickPollMessageComposer : ServerPacket
    {
        public QuickPollMessageComposer(string question) :
            base(ServerPacketHeader.QuestionParserMessageComposer)
        {
            base.WriteString("");
            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(1);//duration
            base.WriteInteger(-1);//id
            base.WriteInteger(120);//number
            base.WriteInteger(3);
            base.WriteString(question);
        }
    }
}
