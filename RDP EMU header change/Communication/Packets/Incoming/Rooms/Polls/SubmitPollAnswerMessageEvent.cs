using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing.Rooms.Polls;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Polls
{
    class SubmitPollAnswerMessageEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            int pollId = Packet.PopInt();
            int questionId = Packet.PopInt();
            int count = Packet.PopInt();
            string answer = Packet.PopString();

            if(questionId == -1)
            {
                if (Session.GetHabbo() == null || Session.GetHabbo().CurrentRoom == null)
                    return;

                Room room = Session.GetHabbo().CurrentRoom;

                if(room.getQuestion() == null)
                {
                    return;
                }

                if (room.getYesVotes().Contains(Session.GetHabbo().Id) || room.getNoVotes().Contains(Session.GetHabbo().Id))
                    return;

                if(answer.Equals("1"))
                {
                    room.getYesVotes().Add(Session.GetHabbo().Id);
                }
                else
                {
                    room.getNoVotes().Add(Session.GetHabbo().Id);
                }

                room.SendMessage(new QuickPollResultsMessageComposer(Session.GetHabbo().Id, answer, room.getYesVotes().Count(), room.getNoVotes().Count()));
                return;
            }
        }
    }
}
