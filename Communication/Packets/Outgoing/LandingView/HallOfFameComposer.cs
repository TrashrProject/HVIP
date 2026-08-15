using Plus.Core;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Plus.Communication.Packets.Outgoing.LandingView
{
    class HallOfFameComposer : ServerPacket
    {
        public HallOfFameComposer()
            : base(ServerPacketHeader.HallOfFameMessageComposer)
        {

            base.WriteString("Ranks");
            

            base.WriteInteger(PlusEnvironment.GetGame().GetHallOfFame().StaffList.Count);
            Console.WriteLine(PlusEnvironment.GetGame().GetHallOfFame().StaffList.Count);
            for (int i = 0; i < PlusEnvironment.GetGame().GetHallOfFame().StaffList.Count; i++)
            {
                base.WriteInteger(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].id);
                Console.WriteLine(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].id);
                base.WriteString(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].username);
                Console.WriteLine(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].username);
                base.WriteString(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].look);
                Console.WriteLine(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].look);
                base.WriteInteger(i);
                Console.WriteLine(i);
                base.WriteInteger(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].rank);
                Console.WriteLine(PlusEnvironment.GetGame().GetHallOfFame().StaffList[i].rank);
            }
        }
    }
}

