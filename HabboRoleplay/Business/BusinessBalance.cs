using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.Business
{
    public class BusinessBalance
    {
        public int ID;
        public int UserId;
        public int GroupId;
        public string Type;
        public int Cant;
        public DateTime TimeStamp;

        public BusinessBalance(int ID, int UserId, int GroupId, string Type, int Cant, DateTime TimeStamp)
        {
            this.ID = ID;
            this.UserId = UserId;
            this.GroupId = GroupId;
            this.Type = Type;
            this.Cant = Cant;
            this.TimeStamp = TimeStamp;
        }

    }
}
