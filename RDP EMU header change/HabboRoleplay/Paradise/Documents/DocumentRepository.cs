using System;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;

namespace Plus.HabboRoleplay.Paradise.Documents
{
    public sealed class DocumentRepository
    {
        public List<ParadiseDocument> LoadForUser(int userId)
        {
            List<ParadiseDocument> documents = new List<ParadiseDocument>();
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("SELECT d.`id`,d.`user_id`,d.`document_type_id`,d.`document_number`,d.`issued_at`,d.`expires_at`,d.`status`,d.`metadata`,t.`code`,t.`name`,t.`category` FROM `rp_player_documents` d INNER JOIN `rp_document_types` t ON t.`id` = d.`document_type_id` WHERE d.`user_id` = @user_id ORDER BY d.`issued_at` ASC");
                db.AddParameter("user_id", userId);
                DataTable table = db.getTable();
                if (table == null) return documents;
                foreach (DataRow row in table.Rows) documents.Add(Map(row));
            }
            return documents;
        }

        public ParadiseDocument Create(int userId, string typeCode, string documentNumber, DateTime? expiresAt, string metadata)
        {
            int typeId;
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("SELECT `id` FROM `rp_document_types` WHERE `code` = @code LIMIT 1");
                db.AddParameter("code", typeCode);
                typeId = db.getInteger();
            }
            if (typeId <= 0) throw new InvalidOperationException("Unknown Paradise document type: " + typeCode);

            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("INSERT INTO `rp_player_documents` (`user_id`,`document_type_id`,`document_number`,`issued_at`,`expires_at`,`status`,`metadata`) VALUES (@user_id,@type_id,@number,NOW(),@expires_at,'VALID',@metadata)");
                db.AddParameter("user_id", userId);
                db.AddParameter("type_id", typeId);
                db.AddParameter("number", documentNumber);
                db.AddParameter("expires_at", expiresAt.HasValue ? (object)expiresAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value);
                db.AddParameter("metadata", String.IsNullOrWhiteSpace(metadata) ? (object)DBNull.Value : metadata);
                db.InsertQuery();
            }

            return FindByCode(userId, typeCode);
        }

        public ParadiseDocument FindByCode(int userId, string typeCode)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("SELECT d.`id`,d.`user_id`,d.`document_type_id`,d.`document_number`,d.`issued_at`,d.`expires_at`,d.`status`,d.`metadata`,t.`code`,t.`name`,t.`category` FROM `rp_player_documents` d INNER JOIN `rp_document_types` t ON t.`id` = d.`document_type_id` WHERE d.`user_id` = @user_id AND t.`code` = @code LIMIT 1");
                db.AddParameter("user_id", userId);
                db.AddParameter("code", typeCode);
                DataRow row = db.getRow();
                return row == null ? null : Map(row);
            }
        }

        public long CreateShare(int senderUserId, int targetUserId, int playerDocumentId, DateTime expiresAt)
        {
            using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                db.SetQuery("INSERT INTO `rp_document_shares` (`sender_user_id`,`target_user_id`,`player_document_id`,`status`,`created_at`,`expires_at`) VALUES (@sender,@target,@document,'PENDING',NOW(),@expires_at)");
                db.AddParameter("sender", senderUserId);
                db.AddParameter("target", targetUserId);
                db.AddParameter("document", playerDocumentId);
                db.AddParameter("expires_at", expiresAt.ToString("yyyy-MM-dd HH:mm:ss"));
                return db.InsertQuery();
            }
        }

        private static ParadiseDocument Map(DataRow row)
        {
            DateTime? expiresAt = null;
            if (row["expires_at"] != DBNull.Value) expiresAt = Convert.ToDateTime(row["expires_at"]);
            return new ParadiseDocument(
                Convert.ToInt32(row["id"]),
                Convert.ToInt32(row["user_id"]),
                Convert.ToInt32(row["document_type_id"]),
                Convert.ToString(row["code"]),
                Convert.ToString(row["name"]),
                Convert.ToString(row["category"]),
                Convert.ToString(row["document_number"]),
                Convert.ToString(row["status"]),
                Convert.ToDateTime(row["issued_at"]),
                expiresAt,
                row["metadata"] == DBNull.Value ? String.Empty : Convert.ToString(row["metadata"]));
        }
    }
}
