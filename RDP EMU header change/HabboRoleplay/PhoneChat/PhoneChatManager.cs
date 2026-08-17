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
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboRoleplay.PhoneChat
{
    public class PhoneChatManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.PhoneChat.PhoneChatManaer");

        /// <summary>
        /// Thread-safe dictionary containing all chats.
        /// </summary>
        public ConcurrentDictionary<int, PhoneChat> ChatList = new ConcurrentDictionary<int, PhoneChat>();

        private const string TableName = "play_phone_chats";

        /// <summary>
        /// Initializes the chat list dictionary and restores every saved phone conversation.
        /// Chats used to live only in RAM, which made WhatsApp discussions disappear after a
        /// client/emulator restart. The database is now the source of persistence.
        /// </summary>
        public void Init()
        {
            ChatList.Clear();

            try
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (dbClient == null)
                    {
                        log.Error("PhoneChat: database unavailable while loading conversations.");
                        return;
                    }

                    dbClient.RunQuery(
                        "CREATE TABLE IF NOT EXISTS `" + TableName + "` (" +
                        "`id` INT NOT NULL AUTO_INCREMENT," +
                        "`type` INT NOT NULL DEFAULT 2," +
                        "`emisor_id` INT NOT NULL," +
                        "`emisor_name` VARCHAR(64) NOT NULL," +
                        "`receptor_id` INT NOT NULL," +
                        "`receptor_name` VARCHAR(64) NOT NULL," +
                        "`msg` TEXT NOT NULL," +
                        "`timestamp` DATETIME NOT NULL," +
                        "PRIMARY KEY (`id`)," +
                        "KEY `idx_phone_sender` (`emisor_id`)," +
                        "KEY `idx_phone_receiver` (`receptor_id`)," +
                        "KEY `idx_phone_type` (`type`)" +
                        ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
                    );

                    dbClient.SetQuery("SELECT `id`,`type`,`emisor_id`,`emisor_name`,`receptor_id`,`receptor_name`,`msg`,`timestamp` FROM `" + TableName + "` ORDER BY `id` ASC");
                    DataTable table = dbClient.getTable();

                    int maxId = 0;
                    if (table != null)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            int id = Convert.ToInt32(row["id"]);
                            int type = Convert.ToInt32(row["type"]);
                            int senderId = Convert.ToInt32(row["emisor_id"]);
                            string senderName = Convert.ToString(row["emisor_name"]);
                            int receiverId = Convert.ToInt32(row["receptor_id"]);
                            string receiverName = Convert.ToString(row["receptor_name"]);
                            string message = Convert.ToString(row["msg"]);
                            DateTime timestamp = Convert.ToDateTime(row["timestamp"]);

                            ChatList.TryAdd(id, new PhoneChat(id, type, senderId, senderName, receiverId, receiverName, message, timestamp));
                            if (id > maxId) maxId = id;
                        }
                    }

                    if (maxId > RoleplayManager.ChatsID)
                        RoleplayManager.ChatsID = maxId;
                }
            }
            catch (Exception e)
            {
                log.Error("PhoneChat persistence initialization failed: " + e);
            }

            log.Info("PhoneChat Initialized 100% - " + ChatList.Count + " conversation message(s) restored");
        }

        public void NewPhoneChat(int ID, int Type, int EmisorId, string EmisorName, int ReceptorId, string ReceptorName, string Msg, DateTime TimeStamp)
        {
            PhoneChat newChat = new PhoneChat(ID, Type, EmisorId, EmisorName, ReceptorId, ReceptorName, Msg, TimeStamp);
            ChatList.TryAdd(ID, newChat);

            try
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (dbClient == null) return;

                    dbClient.SetQuery(
                        "INSERT INTO `" + TableName + "` (`id`,`type`,`emisor_id`,`emisor_name`,`receptor_id`,`receptor_name`,`msg`,`timestamp`) " +
                        "VALUES (@id,@type,@senderId,@senderName,@receiverId,@receiverName,@message,@timestamp) " +
                        "ON DUPLICATE KEY UPDATE `type`=VALUES(`type`),`emisor_id`=VALUES(`emisor_id`),`emisor_name`=VALUES(`emisor_name`)," +
                        "`receptor_id`=VALUES(`receptor_id`),`receptor_name`=VALUES(`receptor_name`),`msg`=VALUES(`msg`),`timestamp`=VALUES(`timestamp`)"
                    );
                    dbClient.AddParameter("id", ID);
                    dbClient.AddParameter("type", Type);
                    dbClient.AddParameter("senderId", EmisorId);
                    dbClient.AddParameter("senderName", EmisorName ?? string.Empty);
                    dbClient.AddParameter("receiverId", ReceptorId);
                    dbClient.AddParameter("receiverName", ReceptorName ?? string.Empty);
                    dbClient.AddParameter("message", Msg ?? string.Empty);
                    dbClient.AddParameter("timestamp", TimeStamp);
                    dbClient.RunQuery();
                }
            }
            catch (Exception e)
            {
                // Never break live messaging if persistence fails; keep the RAM copy alive.
                log.Error("PhoneChat could not persist message " + ID + ": " + e);
            }
        }

        /// <summary>
        /// Always return chats in chronological order. ConcurrentDictionary.Values does not
        /// guarantee an order, which previously made fresh replies appear between older messages.
        /// </summary>
        private List<PhoneChat> Ordered(IEnumerable<PhoneChat> chats)
        {
            return chats
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.ID)
                .ToList();
        }

        public List<PhoneChat> GetPhoneChatsByMyID(int MyId)
        {
            if (MyId == 0)
                return null;

            List<PhoneChat> chats = Ordered(ChatList.Values.Where(x => (x.EmisorId == MyId || x.ReceptorId == MyId) && x.Type == 1));
            return chats.Count > 0 ? chats : null;
        }

        public List<PhoneChat> GetPhoneWhatsChatsByMyID(int MyId)
        {
            if (MyId == 0)
                return null;

            List<PhoneChat> chats = Ordered(ChatList.Values.Where(x => (x.EmisorId == MyId || x.ReceptorId == MyId) && x.Type == 2));
            return chats.Count > 0 ? chats : null;
        }

        public List<PhoneChat> GetPhoneWhatsChatsByChatting(int MyId, int ToId)
        {
            if (MyId == 0)
                return null;

            List<PhoneChat> chats = Ordered(ChatList.Values.Where(x => ((x.EmisorId == MyId && x.ReceptorId == ToId) || (x.EmisorId == ToId && x.ReceptorId == MyId)) && x.Type == 2));
            return chats.Count > 0 ? chats : null;
        }

        public int GetIDbyContact(GameClient Session, string Target)
        {
            int ID = 0;
            Session.GetPlay().SendToName = true;// Envió a nombre de contacto

            if (!int.TryParse(PlusEnvironment.GetUserInfoBy("id", "username", Target), out ID))
            {
                // Limpiamos y dejamos solo numeros
                Target = PlusEnvironment.GetGame().GetClientManager().ClearNumbers(Target);

                if (Target.Length != 10)
                {
                    ID = 0;
                }
                else
                {
                    // Damos Formato al Número (xxx)-xxx-xxxx
                    Target = PlusEnvironment.GetGame().GetClientManager().NumberFormatRP(Target);
                    if (!int.TryParse(PlusEnvironment.GetUserIdByPhoneNumber(Target), out ID))
                    {
                        ID = 0;
                    }
                    Session.GetPlay().SendToName = false;// No Envió a nombre de contacto
                }
            }

            return ID;
        }
    }
}
