using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Relationships;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Outgoing.Messenger
{
    class FriendListUpdateComposer : ServerPacket
    {
        /*REMOVE
         * This is to remove to group or to remove friend. If to remove friend, send friendId as positive
         * If to remove to group, send friendId as negative
         */
        public FriendListUpdateComposer(int FriendId)
            : base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            base.WriteInteger(1);//Category Count
            base.WriteInteger(1);
            base.WriteString("Grupos");
            
            base.WriteInteger(1);//Updates Count

            base.WriteInteger(-1);//remove one
            base.WriteInteger(FriendId);
            
        }
        /*ADD
         * This is to add group  to the friend list.
         * If to add group, send friendId as negative and groupID as positive
         */
        public FriendListUpdateComposer(int FriendId, int groupID)
            :base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            base.WriteInteger(1);//Category Count
            base.WriteInteger(1);
            base.WriteString("Grupos");

            base.WriteInteger(1);//Updates Count

            base.WriteInteger(1);//add one
            if (FriendId <= 0)
            {
                if (groupID != 0)
                {
                    Group group = null;
                    if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupID, out group))
                        return;


                    base.WriteInteger(-group.Id);
                    base.WriteString(group.Name);
                    base.WriteInteger(0);
                    base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                    base.WriteBoolean(false);
                    base.WriteString(group.Badge);
                    base.WriteInteger(1);//category ID
                    base.WriteString(string.Empty);
                    base.WriteString(string.Empty);
                    base.WriteString(string.Empty);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);
                    base.WriteShort(0);
                }

                else
                {
                    base.WriteInteger(-0x7fffffff);
                    base.WriteString("Staff Chat");
                    base.WriteInteger(0);
                    base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                    base.WriteBoolean(false);
                    base.WriteString("b10104s55111s38014");
                    base.WriteInteger(1);//category ID
                    base.WriteString(string.Empty);
                    base.WriteString(string.Empty);
                    base.WriteString(string.Empty);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);
                    base.WriteBoolean(false);
                    base.WriteShort(0);
                }
            }
        }
        
        /*
         * This is to update groups in the friend list
         * 
         */
        public FriendListUpdateComposer(int friendID, int groupID, bool refresh)
            :base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            base.WriteInteger(1);//Category Count
            base.WriteInteger(1);//category ID
            base.WriteString("Grupos");
            base.WriteInteger(1);//Updates Count
            base.WriteInteger(0);//Update
            if (groupID != -0x7fffffff)
            {


                Group group = null;
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupID, out group))
                    return;
                base.WriteInteger(-groupID);
                base.WriteString(group.Name);
                base.WriteInteger(0);
                base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                base.WriteBoolean(false);
                base.WriteString(group.Badge);
                base.WriteInteger(1);//category ID
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteShort(0);
            }
            else
            {
                base.WriteInteger(-0x7fffffff);
                base.WriteString("Staff Chat");
                base.WriteInteger(0);
                base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                base.WriteBoolean(false);
                base.WriteString("b10104s55111s38014");
                base.WriteInteger(1);//category ID
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteShort(0);
            }
        }

        public FriendListUpdateComposer(GameClient Session, MessengerBuddy Buddy)
            : base(ServerPacketHeader.FriendListUpdateMessageComposer)
        {
            base.WriteInteger(1);//Category Count
            base.WriteInteger(1);//category ID
            base.WriteString("Grupos");
            base.WriteInteger(1);//Updates Count
            base.WriteInteger(0);//Update
            if(Buddy.UserId > 0)
            {
                Relationship Relationship = Session.GetHabbo().Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(Buddy.UserId)).Value;
                int y = Relationship == null ? 0 : Relationship.Type;

                base.WriteInteger(Buddy.UserId);
                base.WriteString(Buddy.mUsername);
                base.WriteInteger(1);
                if (!Buddy.mAppearOffline || Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    base.WriteBoolean(Buddy.IsOnline);
                else
                    base.WriteBoolean(false);

                if (!Buddy.mHideInroom || Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
                    base.WriteBoolean(Buddy.InRoom);
                else
                    base.WriteBoolean(false);

                base.WriteString("");//Habbo.IsOnline ? Habbo.Look : "");
                base.WriteInteger(0); // categoryid
                base.WriteString(Buddy.mMotto);
                base.WriteString(string.Empty); // Facebook username
                base.WriteString(string.Empty);
                base.WriteBoolean(true); // Allows offline messaging
                base.WriteBoolean(false); // ?
                base.WriteBoolean(false); // Uses phone
                base.WriteShort(y);
            }

            else if (Buddy.UserId != -0x7fffffff)
            {
                Group group = null;
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(Buddy.groupID, out group))
                    return;
                base.WriteInteger(-Buddy.groupID);
                base.WriteString(group.Name);
                base.WriteInteger(0);
                base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                base.WriteBoolean(false);
                base.WriteString(group.Badge);
                base.WriteInteger(1);//category ID
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteShort(0);
            }

            else
            {
                base.WriteInteger(-0x7fffffff);
                base.WriteString("Staff Chat");
                base.WriteInteger(0);
                base.WriteBoolean(true);//show on the bottom bar the groups? might change this later, seems a bit annoying
                base.WriteBoolean(false);
                base.WriteString("b10104s55111s38014");
                base.WriteInteger(1);//category ID
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteString(string.Empty);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteShort(0);
            }
            
        }
    }
}
