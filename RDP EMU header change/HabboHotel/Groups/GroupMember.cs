using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Groups
{
    public class GroupMember
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }

        public int UserRank { get; set; }
        public bool IsAdmin { get; set; }
        public GroupMember(int GroupId, int UserId, int UserRank, bool IsAdmin)
        {
            this.GroupId = GroupId;
            this.UserId = UserId;
            this.UserRank = UserRank;
            this.IsAdmin = IsAdmin;
        }
    }
}
