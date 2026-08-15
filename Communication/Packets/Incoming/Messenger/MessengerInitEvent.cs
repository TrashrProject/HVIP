using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users.Messenger;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class MessengerInitEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            Session.GetHabbo().GetMessenger().OnStatusChanged(false);

            ICollection<MessengerBuddy> Friends = new List<MessengerBuddy>();
            foreach (MessengerBuddy Buddy in Session.GetHabbo().GetMessenger().GetFriends().ToList())
            {
                if (Buddy == null || Buddy.IsOnline)
                    continue;

                Friends.Add(Buddy);
            }
            foreach (MessengerBuddy Buddy in Session.GetHabbo().GetMessenger().GetGroupChats().ToList())
            {
                if (Buddy == null || Buddy.IsOnline)
                    continue;
                if(Buddy.groupID != -0x7fffffff)
                {
                    Group group = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Buddy.groupID, out group))
                        continue;
                }
                
                Friends.Add(Buddy);
            }
            Session.SendMessage(new MessengerInitComposer());
            Session.SendMessage(new BuddyListComposer(Friends, Session.GetHabbo()));

            Session.GetHabbo().GetMessenger().ProcessOfflineMessages();
            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, "compose_loader|6");// Sum 6 para 90
        }
    }
}