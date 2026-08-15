using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.LandingView.Promotions;
using log4net;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.LandingView
{
    public class LandingViewManager
    {
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboHotel.LandingView.LandingViewManager");

        private Dictionary<int, Promotion> _promotionItems;

        public LandingViewManager()
        {
            this._promotionItems = new Dictionary<int, Promotion>();

            this.LoadPromotions();
        }

        public void LoadPromotions()
        {
            if (this._promotionItems.Count > 0)
                this._promotionItems.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_landing` ORDER BY `id` DESC");
                DataTable GetData = dbClient.getTable();

                if (GetData != null)
                {
                    foreach (DataRow Row in GetData.Rows)
                    {
                        this._promotionItems.Add(Convert.ToInt32(Row[0]), new Promotion((int)Row[0], Row[1].ToString(), Row[2].ToString(), Row[3].ToString(), Convert.ToInt32(Row[4]), Row[5].ToString(), Row[6].ToString()));
                    }
                }
            }


            log.Info("Landing View Manager -> LOADED");
        }

        public ICollection<Promotion> GetPromotionItems()
        {
            return this._promotionItems.Values;
        }


        public bool GenerateCalendarItem(Habbo Habbo, string eventName, int eventDate, out Item newItem)
        {
            newItem = null;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `item_id` FROM `campaign_calendar_gifts` WHERE `event_name` = @eventName AND `base_id` = @dayId LIMIT 1");
                dbClient.AddParameter("eventName", eventName);
                dbClient.AddParameter("dayId", eventDate);

                DataRow row = dbClient.getRow();
                ItemData itemData = null;

                if (row?["item_id"] != null &&
                    PlusEnvironment.GetGame().GetItemManager().GetItem((string)row["item_id"], out itemData))
                {
                    newItem = ItemFactory.CreateSingleItemNullable(itemData, Habbo, "", "");
                    return newItem != null;
                }

                return false;
            }
        }
    }
}