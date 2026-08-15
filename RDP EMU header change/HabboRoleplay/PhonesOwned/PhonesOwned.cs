using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.PhoneOwned
{
    public class PhonesOwned
    {
        public int Id;
        public uint PhoneId;
        public int OwnerId;
        public string PhoneNumber;

        public PhonesOwned(int Id, uint PhoneId, int OwnerId, string PhoneNumber)
        {
            this.Id = Id;
            this.PhoneId = PhoneId;
            this.OwnerId = OwnerId;
            this.PhoneNumber = PhoneNumber;
        }
        
    }
}
