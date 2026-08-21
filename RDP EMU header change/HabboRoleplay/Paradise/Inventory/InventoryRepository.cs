using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.Inventory
{
    public sealed class InventoryRepository
    {
        public List<InventoryItem> LoadForUser(int userId)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                return LoadForUser(db, userId, false);
            }
        }

        public InventoryItem LoadItem(int userId, long itemId)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery(SelectSql + " WHERE i.`owner_user_id`=@owner AND i.`id`=@item_id LIMIT 1");
                db.AddParameter("owner", userId);
                db.AddParameter("item_id", itemId);
                DataRow row = db.getRow();
                return row == null ? null : Map(row);
            }
        }

        public InventoryCapacity GetCapacity(int userId)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                EnsureProfile(db, userId);
                return GetCapacity(db, userId, false);
            }
        }

        public bool ConsumeOne(int userId, long itemId)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("START TRANSACTION");
                db.RunQuery();
                try
                {
                    db.SetQuery(SelectSql + " WHERE i.`owner_user_id`=@owner AND i.`id`=@item_id LIMIT 1 FOR UPDATE");
                    db.AddParameter("owner", userId);
                    db.AddParameter("item_id", itemId);
                    DataRow row = db.getRow();
                    if (row == null)
                    {
                        Rollback(db);
                        return false;
                    }

                    InventoryItem item = Map(row);
                    if (item.Quantity <= 0)
                    {
                        Rollback(db);
                        return false;
                    }

                    if (item.Quantity == 1)
                    {
                        db.SetQuery("DELETE FROM `rp_inventory_items` WHERE `id`=@id AND `owner_user_id`=@owner LIMIT 1");
                        db.AddParameter("id", item.Id);
                        db.AddParameter("owner", userId);
                        db.RunQuery();
                    }
                    else
                    {
                        db.SetQuery("UPDATE `rp_inventory_items` SET `quantity`=`quantity`-1 WHERE `id`=@id AND `owner_user_id`=@owner AND `quantity`>0 LIMIT 1");
                        db.AddParameter("id", item.Id);
                        db.AddParameter("owner", userId);
                        db.RunQuery();
                    }

                    Log(db, "USE", userId, null, item.Id, item.Definition.Id, 1, item.Metadata);
                    db.SetQuery("COMMIT");
                    db.RunQuery();
                    return true;
                }
                catch
                {
                    Rollback(db);
                    return false;
                }
            }
        }

        public bool Transfer(int senderUserId, int targetUserId, long itemId, int quantity, out string message)
        {
            message = null;
            if (senderUserId <= 0 || targetUserId <= 0 || senderUserId == targetUserId || quantity <= 0)
            {
                message = "Transfert invalide.";
                return false;
            }

            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("START TRANSACTION");
                db.RunQuery();
                try
                {
                    EnsureProfile(db, senderUserId);
                    EnsureProfile(db, targetUserId);

                    db.SetQuery(SelectSql + " WHERE i.`owner_user_id`=@owner AND i.`id`=@item_id LIMIT 1 FOR UPDATE");
                    db.AddParameter("owner", senderUserId);
                    db.AddParameter("item_id", itemId);
                    DataRow sourceRow = db.getRow();
                    if (sourceRow == null)
                    {
                        Rollback(db);
                        message = "Objet introuvable dans votre inventaire.";
                        return false;
                    }

                    InventoryItem source = Map(sourceRow);
                    if (source.Definition == null || !source.Definition.Tradeable)
                    {
                        Rollback(db);
                        message = "Cet objet ne peut pas être donné.";
                        return false;
                    }
                    if (quantity > source.Quantity)
                    {
                        Rollback(db);
                        message = "Quantité insuffisante.";
                        return false;
                    }

                    InventoryCapacity targetCapacity = GetCapacity(db, targetUserId, true);
                    List<InventoryItem> targetItems = LoadForUser(db, targetUserId, true);
                    decimal targetWeight = targetItems.Sum(x => x.TotalWeight);
                    decimal movedWeight = source.Definition.Weight * quantity;
                    if (targetWeight + movedWeight > targetCapacity.MaximumWeight)
                    {
                        Rollback(db);
                        message = "Le joueur n’a pas assez de capacité dans son inventaire.";
                        return false;
                    }

                    int neededSlots = CalculateAdditionalSlots(targetItems, source, quantity);
                    if (targetItems.Count + neededSlots > targetCapacity.MaxSlots)
                    {
                        Rollback(db);
                        message = "Le joueur n’a pas assez de place dans son inventaire.";
                        return false;
                    }

                    if (!RemoveQuantity(db, senderUserId, source.Id, source.Quantity, quantity))
                    {
                        Rollback(db);
                        message = "Impossible de retirer l’objet de votre inventaire.";
                        return false;
                    }

                    AddQuantity(db, targetUserId, source.Definition, quantity, source.Metadata);
                    Log(db, "GIVE", senderUserId, targetUserId, source.Id, source.Definition.Id, quantity, source.Metadata);

                    db.SetQuery("COMMIT");
                    db.RunQuery();
                    message = "Objet transféré.";
                    return true;
                }
                catch
                {
                    Rollback(db);
                    message = "Le transfert a été annulé pour protéger les inventaires.";
                    return false;
                }
            }
        }

        public void LogUseWithoutConsume(int userId, InventoryItem item)
        {
            if (item == null || item.Definition == null) return;
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                Log(db, "USE", userId, null, item.Id, item.Definition.Id, 1, item.Metadata);
            }
        }

        private const string SelectSql =
            "SELECT i.`id`,i.`owner_user_id`,i.`quantity`,i.`metadata`,i.`slot`," +
            "d.`id` AS definition_id,d.`code`,d.`name`,d.`description`,d.`category`,d.`weight`,d.`max_stack`,d.`icon`,d.`usable`,d.`tradeable`,d.`droppable`,d.`effect_type`,d.`effect_value`,d.`metadata_schema` " +
            "FROM `rp_inventory_items` i INNER JOIN `rp_item_definitions` d ON d.`id`=i.`item_definition_id`";

        private static List<InventoryItem> LoadForUser(IQueryAdapter db, int userId, bool forUpdate)
        {
            db.SetQuery(SelectSql + " WHERE i.`owner_user_id`=@owner ORDER BY COALESCE(i.`slot`,2147483647),i.`id`" + (forUpdate ? " FOR UPDATE" : String.Empty));
            db.AddParameter("owner", userId);
            DataTable table = db.getTable();
            List<InventoryItem> items = new List<InventoryItem>();
            if (table == null) return items;
            foreach (DataRow row in table.Rows) items.Add(Map(row));
            return items;
        }

        private static void EnsureProfile(IQueryAdapter db, int userId)
        {
            db.SetQuery("INSERT IGNORE INTO `rp_inventory_profiles` (`user_id`) VALUES (@user_id)");
            db.AddParameter("user_id", userId);
            db.RunQuery();
        }

        private static InventoryCapacity GetCapacity(IQueryAdapter db, int userId, bool forUpdate)
        {
            db.SetQuery("SELECT `base_capacity`,`capacity_bonus`,`max_slots` FROM `rp_inventory_profiles` WHERE `user_id`=@user_id LIMIT 1" + (forUpdate ? " FOR UPDATE" : String.Empty));
            db.AddParameter("user_id", userId);
            DataRow row = db.getRow();
            if (row == null)
            {
                return new InventoryCapacity { BaseCapacity = 50m, CapacityBonus = 0m, MaxSlots = 30 };
            }
            return new InventoryCapacity
            {
                BaseCapacity = Convert.ToDecimal(row["base_capacity"]),
                CapacityBonus = Convert.ToDecimal(row["capacity_bonus"]),
                MaxSlots = Math.Max(1, Convert.ToInt32(row["max_slots"]))
            };
        }

        private static int CalculateAdditionalSlots(List<InventoryItem> targetItems, InventoryItem source, int quantity)
        {
            int remaining = quantity;
            int maxStack = Math.Max(1, source.Definition.MaxStack);
            if (maxStack > 1)
            {
                foreach (InventoryItem target in targetItems.Where(x => x.Definition != null && x.Definition.Id == source.Definition.Id && SameMetadata(x.Metadata, source.Metadata)))
                {
                    int space = Math.Max(0, maxStack - target.Quantity);
                    remaining -= Math.Min(space, remaining);
                    if (remaining <= 0) return 0;
                }
            }
            return (int)Math.Ceiling(remaining / (decimal)maxStack);
        }

        private static bool RemoveQuantity(IQueryAdapter db, int ownerUserId, long itemId, int current, int quantity)
        {
            if (quantity == current)
            {
                db.SetQuery("DELETE FROM `rp_inventory_items` WHERE `id`=@id AND `owner_user_id`=@owner AND `quantity`=@quantity LIMIT 1");
            }
            else
            {
                db.SetQuery("UPDATE `rp_inventory_items` SET `quantity`=`quantity`-@move WHERE `id`=@id AND `owner_user_id`=@owner AND `quantity`>=@move LIMIT 1");
                db.AddParameter("move", quantity);
            }
            db.AddParameter("id", itemId);
            db.AddParameter("owner", ownerUserId);
            db.AddParameter("quantity", current);
            db.RunQuery();
            return true;
        }

        private static void AddQuantity(IQueryAdapter db, int targetUserId, ItemDefinition definition, int quantity, string metadata)
        {
            int remaining = quantity;
            int maxStack = Math.Max(1, definition.MaxStack);
            string normalizedMetadata = metadata ?? String.Empty;

            if (maxStack > 1)
            {
                db.SetQuery("SELECT `id`,`quantity` FROM `rp_inventory_items` WHERE `owner_user_id`=@owner AND `item_definition_id`=@definition AND COALESCE(`metadata`,'')=@metadata AND `quantity`<@max_stack ORDER BY `id` FOR UPDATE");
                db.AddParameter("owner", targetUserId);
                db.AddParameter("definition", definition.Id);
                db.AddParameter("metadata", normalizedMetadata);
                db.AddParameter("max_stack", maxStack);
                DataTable stacks = db.getTable();
                if (stacks != null)
                {
                    foreach (DataRow row in stacks.Rows)
                    {
                        if (remaining <= 0) break;
                        long id = Convert.ToInt64(row["id"]);
                        int current = Convert.ToInt32(row["quantity"]);
                        int add = Math.Min(maxStack - current, remaining);
                        if (add <= 0) continue;
                        db.SetQuery("UPDATE `rp_inventory_items` SET `quantity`=`quantity`+@add WHERE `id`=@id LIMIT 1");
                        db.AddParameter("add", add);
                        db.AddParameter("id", id);
                        db.RunQuery();
                        remaining -= add;
                    }
                }
            }

            while (remaining > 0)
            {
                int add = Math.Min(maxStack, remaining);
                db.SetQuery("INSERT INTO `rp_inventory_items` (`owner_user_id`,`item_definition_id`,`quantity`,`metadata`) VALUES (@owner,@definition,@quantity,@metadata)");
                db.AddParameter("owner", targetUserId);
                db.AddParameter("definition", definition.Id);
                db.AddParameter("quantity", add);
                db.AddParameter("metadata", metadata == null ? (object)DBNull.Value : metadata);
                db.InsertQuery();
                remaining -= add;
            }
        }

        private static void Log(IQueryAdapter db, string type, int actor, int? target, long? itemId, int? definitionId, int quantity, string metadata)
        {
            db.SetQuery("INSERT INTO `rp_inventory_transactions` (`transaction_type`,`actor_user_id`,`target_user_id`,`inventory_item_id`,`item_definition_id`,`quantity`,`metadata`) VALUES (@type,@actor,@target,@item,@definition,@quantity,@metadata)");
            db.AddParameter("type", type);
            db.AddParameter("actor", actor);
            db.AddParameter("target", target.HasValue ? (object)target.Value : DBNull.Value);
            db.AddParameter("item", itemId.HasValue ? (object)itemId.Value : DBNull.Value);
            db.AddParameter("definition", definitionId.HasValue ? (object)definitionId.Value : DBNull.Value);
            db.AddParameter("quantity", quantity);
            db.AddParameter("metadata", metadata == null ? (object)DBNull.Value : metadata);
            db.InsertQuery();
        }

        private static void Rollback(IQueryAdapter db)
        {
            try
            {
                db.SetQuery("ROLLBACK");
                db.RunQuery();
            }
            catch { }
        }

        private static bool SameMetadata(string a, string b)
        {
            return String.Equals(a ?? String.Empty, b ?? String.Empty, StringComparison.Ordinal);
        }

        private static InventoryItem Map(DataRow row)
        {
            ItemDefinition definition = new ItemDefinition(
                Convert.ToInt32(row["definition_id"]),
                Convert.ToString(row["code"]),
                Convert.ToString(row["name"]),
                Convert.ToString(row["description"]),
                Convert.ToString(row["category"]),
                Convert.ToDecimal(row["weight"]),
                Convert.ToInt32(row["max_stack"]),
                row["icon"] == DBNull.Value ? null : Convert.ToString(row["icon"]),
                Convert.ToBoolean(row["usable"]),
                Convert.ToBoolean(row["tradeable"]),
                Convert.ToBoolean(row["droppable"]),
                Convert.ToString(row["effect_type"]),
                Convert.ToInt32(row["effect_value"]),
                row["metadata_schema"] == DBNull.Value ? null : Convert.ToString(row["metadata_schema"]));

            int? slot = row["slot"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["slot"]);
            return new InventoryItem(
                Convert.ToInt64(row["id"]),
                Convert.ToInt32(row["owner_user_id"]),
                definition,
                Convert.ToInt32(row["quantity"]),
                row["metadata"] == DBNull.Value ? null : Convert.ToString(row["metadata"]),
                slot);
        }
    }
}
