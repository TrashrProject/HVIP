using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using log4net;

namespace Plus.HabboRoleplay.Business
{
    public class BusinessBalanceManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Business.BusinessBalanceManager");

        /// <summary>
        /// Thread-safe dictionary containing all apartments list
        /// </summary>
        public static ConcurrentDictionary<int, BusinessBalance> BusinessBalances = new ConcurrentDictionary<int, BusinessBalance>();

        /// <summary>
        /// Initializes the apartment list dictionary
        /// </summary>
        public void Init()
        {
            BusinessBalances.Clear();
            DataTable AP;

            using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DB.SetQuery("SELECT * from `play_jobs_balance`");
                AP = DB.getTable();

                if (AP != null)
                {
                    foreach (DataRow Row in AP.Rows)
                    {
                        int ID = Convert.ToInt32(Row["id"]);
                        int UserId = Convert.ToInt32(Row["user_id"]);
                        int GroupId = Convert.ToInt32(Row["group_id"]);
                        string Type = Convert.ToString(Row["type"]);
                        int Cant = Convert.ToInt32(Row["cant"]);
                        DateTime TimeStamp = PlusEnvironment.UnixTimeStampToDateTime(Convert.ToDouble(Row["timestamp"]));

                        BusinessBalance newBalance = new BusinessBalance(ID, UserId, GroupId, Type, Cant, TimeStamp);
                        BusinessBalances.TryAdd(ID, newBalance);
                    }
                }
            }

            log.Info("Loaded " + BusinessBalances.Count + " business balance(s).");
        }

        public List<BusinessBalance> GetBusinessBalance()
        {
            return BusinessBalances.Values.ToList();
        }

        public List<BusinessBalance> GetBusinessBalanceByGroupId(int GroupId)
        {
            return BusinessBalances.Values.Where(x => x.GroupId == GroupId).ToList();
        }

        public void AddBusinessBalance(int UserId, int GroupId, string Type, int Cant)
        {
            int ID = 0;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `play_jobs_balance` (`user_id`, `group_id`, `type`, `cant`, `timestamp`) VALUES(@userid, @groupid, @type, @cant, @timestamp)");
                dbClient.AddParameter("userid", UserId);
                dbClient.AddParameter("groupid", GroupId);
                dbClient.AddParameter("type", Type);
                dbClient.AddParameter("cant", Cant);
                dbClient.AddParameter("timestamp", PlusEnvironment.GetUnixTimestamp());

                ID = Convert.ToInt32(dbClient.InsertQuery());
            }
            if (ID > 0)
            {
                BusinessBalance newBalance = new BusinessBalance(ID, UserId, GroupId, Type, Cant, DateTime.Now);
                BusinessBalances.TryAdd(ID, newBalance);
            }
        }

        public void DeleteBalance(int GroupId, bool ToDB = false)
        {
            List<BusinessBalance> Balances = GetBusinessBalanceByGroupId(GroupId);

            if (Balances == null || Balances.Count <= 0)
                return;

            foreach (var B in Balances)
            {
                if (B.GroupId != GroupId)
                    continue;

                if (BusinessBalances.ContainsKey(B.ID))
                    BusinessBalances.TryRemove(B.ID, out BusinessBalance VO);
            }

            if (ToDB)
            {
                using (var DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    DB.RunQuery("DELETE FROM `play_jobs_balance` WHERE `play_jobs_balance`.`group_id` = '" + GroupId + "'");
                }
            }
        }
    }
}
