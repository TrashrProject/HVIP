using System;
using System.Text.RegularExpressions;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    /// <summary>
    /// Legacy :sms entry point kept for emulator compatibility.
    /// ParadisePhone V1 reuses PhoneChatManager/play_phone_chats instead of creating
    /// a second emulator chat subsystem. The new 555-XXXX identity is resolved in SQL.
    /// </summary>
    class SmsCommand : IChatCommand
    {
        public string PermissionRequired { get { return "command_sms"; } }
        public string Parameters { get { return "<numero> <message>"; } }
        public string Description { get { return "Envoie un SMS privé avec ParadisePhone."; } }

        public void Execute(GameClient session, Room room, string[] parameters)
        {
            if (session == null || session.GetHabbo() == null) return;
            if (parameters == null || parameters.Length < 3)
            {
                session.SendWhisper("[TÉLÉPHONE] Syntaxe : :sms <numéro> <message>", 1);
                return;
            }

            int senderId = session.GetHabbo().Id;
            if (!HasPhysicalPhone(senderId))
            {
                session.SendWhisper("[TÉLÉPHONE] Vous ne possédez pas de téléphone.", 1);
                return;
            }
            if (session.GetPlay() != null && session.GetPlay().TryGetCooldown("msg", true))
            {
                session.SendWhisper("[TÉLÉPHONE] Veuillez patienter avant d'envoyer un nouveau message.", 1);
                return;
            }

            string targetToken = Regex.Replace(parameters[1] ?? String.Empty, "<(.|\\n)*?>", String.Empty).Trim();
            string message = Regex.Replace(CommandManager.MergeParams(parameters, 2), "<(.|\\n)*?>", String.Empty).Trim();
            if (message.Length == 0 || message.Length > 500)
            {
                session.SendWhisper("[ERREUR] Le message doit contenir entre 1 et 500 caractères.", 1);
                return;
            }

            int targetId;
            string targetNumber;
            string targetName;
            if (!TryResolveTarget(senderId, targetToken, out targetId, out targetNumber, out targetName))
            {
                session.SendWhisper("[ERREUR] Ce numéro/contact est indisponible.", 1);
                return;
            }
            if (targetId == senderId)
            {
                session.SendWhisper("[ERREUR] Vous ne pouvez pas vous envoyer un SMS à vous-même.", 1);
                return;
            }

            int id = RoleplayManager.ChatsID += 1;
            DateTime sentAt = DateTime.Now;
            string senderName = session.GetHabbo().Username;
            PlusEnvironment.GetGame().GetPhoneChatManager().NewPhoneChat(id, 1, senderId, senderName, targetId, targetName, message, sentAt);

            // PhoneChatManager persists the authoritative SMS in play_phone_chats.
            // Add read state fields introduced by Phase 4 without duplicating the message.
            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    db.SetQuery("UPDATE `play_phone_chats` SET `status`='SENT',`read_at`=NULL WHERE `id`=@id LIMIT 1");
                    db.AddParameter("id", id);
                    db.RunQuery();

                    int targetPhoneId = GetPhoneIdByUser(targetId);
                    if (targetPhoneId > 0)
                    {
                        db.SetQuery("INSERT INTO `rp_phone_notifications` (`phone_id`,`notification_type`,`title`,`body`,`metadata`) VALUES (@phone,'MESSAGE','Nouveau message',@body,@metadata)");
                        db.AddParameter("phone", targetPhoneId);
                        db.AddParameter("body", senderName + " : " + (message.Length > 90 ? message.Substring(0, 90) : message));
                        db.AddParameter("metadata", "{\"chat_id\":" + id + "}");
                        db.RunQuery();
                    }
                }
            }
            catch { }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(targetId);
            if (targetClient != null && !targetClient.LoggingOut)
                targetClient.SendWhisper("[TÉLÉPHONE] Nouveau message de " + senderName + ".", 1);

            session.SendWhisper("[TÉLÉPHONE] Message envoyé à " + targetName + ".", 1);
            if (session.GetPlay() != null)
                session.GetPlay().CooldownManager.CreateCooldown("msg", 1000, 3);
        }

        private static bool HasPhysicalPhone(int userId)
        {
            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    db.SetQuery("SELECT COUNT(*) FROM `rp_inventory_items` i INNER JOIN `rp_item_definitions` d ON d.`id`=i.`item_definition_id` WHERE i.`owner_user_id`=@uid AND i.`quantity`>0 AND (UPPER(d.`effect_type`)='PHONE' OR UPPER(d.`code`)='PHONE_BASIC')");
                    db.AddParameter("uid", userId);
                    return db.getInteger() > 0;
                }
            }
            catch { return false; }
        }

        private static int GetPhoneIdByUser(int userId)
        {
            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    db.SetQuery("SELECT `id` FROM `rp_phones` WHERE `user_id`=@uid AND `status`='ACTIVE' LIMIT 1");
                    db.AddParameter("uid", userId);
                    return db.getInteger();
                }
            }
            catch { return 0; }
        }

        private static bool TryResolveTarget(int senderUserId, string token, out int targetUserId, out string number, out string displayName)
        {
            targetUserId = 0;
            number = null;
            displayName = null;
            if (String.IsNullOrWhiteSpace(token)) return false;

            try
            {
                using (IQueryAdapter db = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    db.SetQuery("SELECT `user_id`,`phone_number` FROM `rp_phones` WHERE `phone_number`=@token AND `status`='ACTIVE' LIMIT 1");
                    db.AddParameter("token", token.Trim());
                    System.Data.DataRow row = db.getRow();
                    if (row == null)
                    {
                        db.SetQuery("SELECT p.`user_id`,p.`phone_number`,c.`display_name` FROM `rp_phones` mine INNER JOIN `rp_phone_contacts` c ON c.`phone_id`=mine.`id` INNER JOIN `rp_phones` p ON p.`phone_number`=c.`contact_phone_number` WHERE mine.`user_id`=@uid AND LOWER(c.`display_name`)=LOWER(@token) AND p.`status`='ACTIVE' LIMIT 1");
                        db.AddParameter("uid", senderUserId);
                        db.AddParameter("token", token.Trim());
                        row = db.getRow();
                    }
                    if (row == null) return false;
                    targetUserId = Convert.ToInt32(row["user_id"]);
                    number = Convert.ToString(row["phone_number"]);
                    displayName = row.Table.Columns.Contains("display_name") && row["display_name"] != DBNull.Value
                        ? Convert.ToString(row["display_name"])
                        : PlusEnvironment.GetUserInfoBy("username", "id", Convert.ToString(targetUserId));
                    if (String.IsNullOrWhiteSpace(displayName)) displayName = number;
                    return targetUserId > 0;
                }
            }
            catch { return false; }
        }
    }
}
