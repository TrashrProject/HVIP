using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;

using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.GangTurfs
{
    public class GangTurfs
    {
        public int RoomId;
        public int GangIdOwner;

        public GangTurfs(int RoomId, int GangIdOwner)
        {
            this.RoomId = RoomId;
            this.GangIdOwner = GangIdOwner;
        }
        
    }
}
