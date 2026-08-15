using Plus.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboHotel.Users.UserDataManagement;

namespace Plus.Communication.Packets.Outgoing.Inventory.Purse
{
    class HabboClubCenterInfoMessageComposer : ServerPacket
    {
        public HabboClubCenterInfoMessageComposer() : base(ServerPacketHeader.HabboClubCenterInfoMessageComposer)
        {
            DataRow Query = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `account_created` FROM `users`");
                Query = dbClient.getRow();
            }
            base.WriteInteger(5);
            base.WriteString(Convert.ToString(Math.Ceiling((PlusEnvironment.GetUnixTimestamp() - Convert.ToDouble(Query["account_created"])) / 60)));
            base.WriteInteger(1069128089); // this should be a double?
            base.WriteInteger(-1717986918);
            base.WriteInteger(0);//unused
            base.WriteInteger(0); // unused
            base.WriteInteger(10); //Credits spent for buying
            base.WriteInteger(20); //streak bonus
            base.WriteInteger(10); //Total credits received 
            base.WriteInteger(60); //Minutes for wait 
        }
    }
}