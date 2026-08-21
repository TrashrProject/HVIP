using System;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.UI
{
    public static class ParadiseUiEventService
    {
        public static void OpenProfile(int userId, string tab, string documentCode = null)
        {
            string payload = "tab=" + Encode(tab);
            if (!String.IsNullOrWhiteSpace(documentCode))
                payload += "&document=" + Encode(documentCode);

            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("INSERT INTO `rp_ui_events` (`user_id`,`event_type`,`payload`,`status`,`created_at`,`expires_at`) VALUES (@user_id,'PROFILE_OPEN',@payload,'PENDING',NOW(),DATE_ADD(NOW(), INTERVAL 90 SECOND))");
                db.AddParameter("user_id", userId);
                db.AddParameter("payload", payload);
                db.InsertQuery();
            }
        }

        private static string Encode(string value)
        {
            return Uri.EscapeDataString(value ?? String.Empty);
        }
    }
}
