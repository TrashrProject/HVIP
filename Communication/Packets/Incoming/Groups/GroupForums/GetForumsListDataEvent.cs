using System.Linq;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Groups.Forums;
using Plus.Communication.Packets.Outgoing.Groups;

namespace Plus.Communication.Packets.Incoming.Groups.Forums
{
    class GetForumsListDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int type = packet.PopInt();
            int startIndex = packet.PopInt();
            int unknown = packet.PopInt();

            List<Group> groups = new List<Group>();
            switch (type)
            {
                default:
                case 0: // Most Active.
                    groups = PlusEnvironment.GetGame().GetGroupManager().GetActiveGroupForums();
                    break;

                case 1: // Most Viewed.
                    groups = PlusEnvironment.GetGame().GetGroupManager().GetPopularGroupForums();
                    break;

                case 2: // My Forums
                    groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupForumsForUser(session.GetHabbo().Id).OrderByDescending(x => x.GetForum().LastReplierDate).ToList();
                    break;
            }

            session.SendMessage(new ForumsListDataComposer(groups.Skip(startIndex).Take(groups.Count - startIndex).Take(20).ToList(), type, startIndex, groups.Count()));
        }
    }
}