using Plus.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.LandingView
{
   public class HallOfFame
    {

            public List<hallOfFameWinner> StaffList;
            public HallOfFame()
            {
                StaffList = new List<hallOfFameWinner>();
                StaffList.Clear();

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `username`,`look`,`rank`,`id` FROM `users` WHERE `rank` > '1' ORDER BY `rank` DESC");
                    DataTable staff = dbClient.getTable();

                    if (staff != null)
                    {
                        foreach (DataRow Data in staff.Rows)
                        {
                            StaffList.Add(new hallOfFameWinner(Data["username"].ToString(), Convert.ToInt32(Data["rank"]), Data["look"].ToString(), Convert.ToInt32(Data["id"])));
                        }
                    }
                }
                return;
            }

        

        public class hallOfFameWinner
        {
            public string username;
            public int rank;
            public string look;
            public int id;

            public hallOfFameWinner(string username, int rank, string look, int id)
            {
                this.username = username;
                this.rank = rank;
                this.look = look;
                this.id = id;
            }
        }
    }
}
