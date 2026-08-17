using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Moderation;
using Plus.HabboHotel.Support;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    class ModeratorInitComposer : ServerPacket
    {

        public ModeratorInitComposer(ICollection<string> UserPresets, ICollection<string> RoomPresets, Dictionary<string, List<ModerationPresetActionMessages>> UserActionPresets, ICollection<SupportTicket> Tickets)
 : base(ServerPacketHeader.ModeratorInitMessageComposer)
        {
            base.WriteInteger(Tickets.Count);
            foreach (SupportTicket ticket in Tickets.ToList())
            {
                base.WriteInteger(ticket.Id);
                base.WriteInteger(ticket.TabId);
                base.WriteInteger(1); // Type
                base.WriteInteger(ticket.Category); // Category
                base.WriteInteger(((int)PlusEnvironment.GetUnixTimestamp() - Convert.ToInt32(ticket.Timestamp)) * 1000);
                base.WriteInteger(ticket.Score);
                base.WriteInteger(0);
                base.WriteInteger(ticket.SenderId);
                base.WriteString(ticket.SenderName);
                base.WriteInteger(ticket.ReportedId);
                base.WriteString(ticket.ReportedName);
                base.WriteInteger((ticket.Status == TicketStatus.PICKED) ? ticket.ModeratorId : 0);
                base.WriteString(ticket.ModName);
                base.WriteString(ticket.Message);
                base.WriteInteger(0);
                base.WriteInteger(0);
            }


            base.WriteInteger(UserPresets.Count);
            foreach (string pre in UserPresets)
            {
                base.WriteString(pre);
            }


            base.WriteInteger(UserActionPresets.Count);
            foreach (KeyValuePair<string, List<ModerationPresetActionMessages>> Cat in UserActionPresets.ToList())
            {
                base.WriteString(Cat.Key);
            }


            base.WriteBoolean(true); // Ticket right
            base.WriteBoolean(true); // Chatlogs
            base.WriteBoolean(true); // User actions alert etc
            base.WriteBoolean(true); // Kick users
            base.WriteBoolean(true); // Ban users
            base.WriteBoolean(true); // Caution etc
            base.WriteBoolean(true); // Love you, Tom


            base.WriteInteger(RoomPresets.Count);
            foreach (string pre in RoomPresets)
            {
                base.WriteString(pre);
            }
        }
    }
   }