using System;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    public static class InventoryUiEventService
    {
        public static bool OpenInventory(int userId)
        {
            return Push(userId, "INVENTORY_OPEN", "");
        }

        public static bool Toast(int userId, string title, string message)
        {
            string payload = "title=" + Encode(title) + "&message=" + Encode(message);
            return Push(userId, "INVENTORY_TOAST", payload);
        }

        public static bool OpenPhone(int userId)
        {
            return Push(userId, "PHONE_OPEN", "source=inventory");
        }

        private static bool Push(int userId, string type, string payload)
        {
            if (userId <= 0 || String.IsNullOrWhiteSpace(type)) return false;
            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    db.SetQuery("INSERT INTO `rp_ui_events` (`user_id`,`event_type`,`payload`,`status`,`created_at`,`expires_at`) VALUES (@user_id,@type,@payload,'PENDING',NOW(),DATE_ADD(NOW(), INTERVAL 90 SECOND))");
                    db.AddParameter("user_id", userId);
                    db.AddParameter("type", type);
                    db.AddParameter("payload", payload ?? String.Empty);
                    db.InsertQuery();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Encode(string value)
        {
            return Uri.EscapeDataString(value ?? String.Empty);
        }
    }
}
