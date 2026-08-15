using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    class NewConsoleMessageComposer : ServerPacket
    {
        public NewConsoleMessageComposer(int Sender, string Message, int Time = 0)
            : base(ServerPacketHeader.NewConsoleMessageMessageComposer)
        {
            base.WriteInteger(Sender);
           base.WriteString(Message);
            base.WriteInteger(Time);
        }

        public NewConsoleMessageComposer(int groupID, string Message, string sender, int Time = 0)
            : base(ServerPacketHeader.NewConsoleMessageMessageComposer)
        {
            base.WriteInteger(groupID);
            base.WriteString(Message);
            base.WriteInteger(Time);
            base.WriteString(sender);
        }
    }
}
