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

        /// <summary>
        /// Initializes the chat list dictionary.
        /// </summary>
        public void Init()
        {
            ChatList.Clear();
            log.Info("PhoneChat Initialized 100%");
        }

        public void NewPhoneChat(int ID, int Type, int EmisorId, string EmisorName, int ReceptorId, string ReceptorName, string Msg, DateTime TimeStamp)
        {
            PhoneChat newChat = new PhoneChat(ID, Type, EmisorId, EmisorName, ReceptorId, ReceptorName, Msg, TimeStamp);
            ChatList.TryAdd(ID, newChat);
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
