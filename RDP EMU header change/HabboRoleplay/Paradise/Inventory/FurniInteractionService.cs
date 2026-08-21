using System;
using System.Data;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    /// <summary>
    /// Server-side opt-in mapping for Habbo furnis that become RP interactions.
    /// Phase 3 intentionally does not patch the Nitro renderer or native furni packets.
    /// A live hook can call this service only after a verified existing furni event is identified.
    /// </summary>
    public static class FurniInteractionService
    {
        public static bool TryGetInteraction(int baseItemId, out string interactionType, out string configuration)
        {
            interactionType = null;
            configuration = null;
            if (baseItemId <= 0) return false;

            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    db.SetQuery("SELECT `interaction_type`,`configuration` FROM `rp_furni_interactions` WHERE `base_item_id`=@base_item_id AND `enabled`=1 LIMIT 1");
                    db.AddParameter("base_item_id", baseItemId);
                    DataRow row = db.getRow();
                    if (row == null) return false;
                    interactionType = Convert.ToString(row["interaction_type"]).Trim().ToUpperInvariant();
                    configuration = row["configuration"] == DBNull.Value ? null : Convert.ToString(row["configuration"]);
                    return interactionType.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static long GetOrCreateFurniContainer(long roomItemId, int roomId, int baseItemId, int ownerId)
        {
            if (roomItemId <= 0 || roomId <= 0 || baseItemId <= 0 || ownerId <= 0) return 0;
            string type;
            string config;
            if (!TryGetInteraction(baseItemId, out type, out config) || !String.Equals(type, "STORAGE", StringComparison.OrdinalIgnoreCase)) return 0;

            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("SELECT `container_id` FROM `rp_furni_containers` WHERE `room_item_id`=@room_item_id LIMIT 1");
                db.AddParameter("room_item_id", roomItemId);
                int existing = db.getInteger();
                if (existing > 0) return existing;

                db.SetQuery("START TRANSACTION");
                db.RunQuery();
                try
                {
                    db.SetQuery("SELECT `container_id` FROM `rp_furni_containers` WHERE `room_item_id`=@room_item_id LIMIT 1 FOR UPDATE");
                    db.AddParameter("room_item_id", roomItemId);
                    existing = db.getInteger();
                    if (existing > 0)
                    {
                        db.SetQuery("COMMIT");
                        db.RunQuery();
                        return existing;
                    }

                    db.SetQuery("INSERT INTO `rp_containers` (`container_type`,`owner_type`,`owner_id`,`capacity`,`max_slots`,`metadata`) VALUES ('CHEST','PLAYER',@owner_id,100.000,40,@metadata)");
                    db.AddParameter("owner_id", ownerId);
                    db.AddParameter("metadata", String.IsNullOrWhiteSpace(config) ? (object)DBNull.Value : config);
                    long containerId = db.InsertQuery();
                    if (containerId <= 0)
                    {
                        db.SetQuery("ROLLBACK");
                        db.RunQuery();
                        return 0;
                    }

                    db.SetQuery("INSERT INTO `rp_furni_containers` (`room_item_id`,`room_id`,`base_item_id`,`container_id`) VALUES (@room_item_id,@room_id,@base_item_id,@container_id)");
                    db.AddParameter("room_item_id", roomItemId);
                    db.AddParameter("room_id", roomId);
                    db.AddParameter("base_item_id", baseItemId);
                    db.AddParameter("container_id", containerId);
                    db.RunQuery();
                    db.SetQuery("COMMIT");
                    db.RunQuery();
                    return containerId;
                }
                catch
                {
                    try { db.SetQuery("ROLLBACK"); db.RunQuery(); } catch { }
                    return 0;
                }
            }
        }
    }
}
