
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    class BuddyListComposer : ServerPacket
    {
        public BuddyListComposer(ICollection<MessengerBuddy> Friends, Habbo Player)
            : base(ServerPacketHeader.BuddyListMessageComposer)
        {
            base.WriteInteger(1);
            base.WriteInteger(0);

            base.WriteInteger(Friends.Count);
            foreach (MessengerBuddy Friend in Friends.ToList())
            {
                //staff chat
                if (Friend.Id == -0x7fffffff)
                {
                    base.WriteInteger(-0x7fffffff);
                    base.WriteString("Staff Chat");
                    base.WriteInteger(0);//Gender.
                    base.WriteBoolean(true);
                    base.WriteBoolean(false);
                    base.WriteString("b10104s55111s38014");
                    base.WriteInteger(1); // category id
                    base.WriteString(string.Empty);
                    base.WriteString(string.Empty);//Alternative name?
                    base.WriteString(string.Empty);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);//Pocket Habbo user.
                    base.WriteShort(0);


                }
                /*
                 * LEFT OFF RIGHT HERE, UNFINISHED
                 * 
                */
                else if(Friend.Id < 0)
                {
                    /*
                    Group group = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Friend.groupID, out group))
                        return;
                    if(group.GroupChatEnabled == 1)
                    {
                    */
                        base.WriteInteger(Friend.Id);
                        base.WriteString(Friend.mUsername);
                        base.WriteInteger(0);
                        base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                        base.WriteBoolean(false);
                        base.WriteString(Friend.mLook);
                        base.WriteInteger(1);//category ID
                        base.WriteString(string.Empty);
                        base.WriteString(string.Empty);
                        base.WriteString(string.Empty);
                        base.WriteBoolean(false);
                        base.WriteBoolean(false);
                        base.WriteBoolean(false);
                        base.WriteShort(0);
                    //}
                }
                else
                {
                    Relationship Relationship = Player.Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(Friend.UserId)).Value;

                    base.WriteInteger(Friend.Id);
                    base.WriteString(Friend.mUsername);
                    base.WriteInteger(1);//Gender.
                    base.WriteBoolean(Friend.IsOnline);
                    base.WriteBoolean(Friend.IsOnline && Friend.InRoom);
                    base.WriteString(Friend.IsOnline ? Friend.mLook : string.Empty);
                    base.WriteInteger(0); // category id
                    base.WriteString(Friend.IsOnline ? Friend.mMotto : string.Empty);
                    base.WriteString(string.Empty);//Alternative name?
                    base.WriteString(string.Empty);
                    base.WriteBoolean(true);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);//Pocket Habbo user.
                    base.WriteShort(Relationship == null ? 0 : Relationship.Type);
                }
            }
        }
    }
}