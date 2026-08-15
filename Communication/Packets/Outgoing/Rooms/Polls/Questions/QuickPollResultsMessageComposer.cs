using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Rooms.Polls
{
    class QuickPollResultsMessageComposer : ServerPacket
    {
        public QuickPollResultsMessageComposer(int playerId, string myVote, int yesVotesCount, int noVotesCount) :
            base(ServerPacketHeader.QuestionAnswersMessageComposer)
        {
            base.WriteInteger(playerId);
            base.WriteString(myVote);
            base.WriteInteger(2);
            base.WriteString("1");
            base.WriteInteger(yesVotesCount);
            base.WriteString("0");
            base.WriteInteger(noVotesCount);
        }

        public QuickPollResultsMessageComposer(int yesVotesCount, int noVotesCount) : base(ServerPacketHeader.EndPollMessageComposer)
        {
            base.WriteInteger(-1);
            base.WriteInteger(2);
            base.WriteString("1");
            base.WriteInteger(yesVotesCount);
            base.WriteString("0");
            base.WriteInteger(noVotesCount);
        }
    }
}
