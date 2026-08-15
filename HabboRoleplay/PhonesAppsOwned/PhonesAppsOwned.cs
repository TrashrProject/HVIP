using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.PhoneAppOwned
{
    public class PhonesAppsOwned
    {
        public int Id;
        public int PhoneId;
        public int AppId;
        public int ScreenId;
        public int SlotId;
        public string Extradata;

        public PhonesAppsOwned(int Id, int PhoneId, int AppId, int ScreenId, int SlotId, string Extradata)
        {
            this.Id = Id;
            this.PhoneId = PhoneId;
            this.AppId = AppId;
            this.ScreenId = ScreenId;
            this.SlotId = SlotId;
            this.Extradata = Extradata;
        }
        
    }
}
