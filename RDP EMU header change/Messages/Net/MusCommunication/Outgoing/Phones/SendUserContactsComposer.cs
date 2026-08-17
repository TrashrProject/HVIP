
using Newtonsoft.Json;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plus.Messages.Net.MusCommunication.Outgoing.Phones
{
    class SendUserContactsComposer : MusPacketEvent
    {
        // ID can be APPID or WebID (Check if user have acces to this ID)
        public SendUserContactsComposer(int ID = 0, int UserID = 0)
        {
            List<ErrorStructure> E = new List<ErrorStructure>();
            E.Add(new ErrorStructure { Error = "true", Code = "5000", Message = "No se encontraron datos para enviar." });

            // Incoming Information to send to Client
            PacketName = "event_sendusercontacts";
            PacketData = JsonConvert.SerializeObject(E);

            List<ResultStructure> L = new List<ResultStructure>();

            GameClient Client = null;
            if (PlusEnvironment.GetGame() != null && PlusEnvironment.GetGame().GetClientManager() != null)
                Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(UserID);

            if (Client != null)
            {
                if (Client.GetPlay().Phone <= 0)
                    return;

                if (!Client.GetPlay().OwnedPhonesApps.ContainsKey(ID))
                    return;

                List<MessengerBuddy> Contacts = Client.GetHabbo().GetMessenger().GetFriends().ToList();
                foreach (MessengerBuddy Buddy in Contacts)
                {
                    if (Buddy != null)
                    {
                        string Number = PlusEnvironment.GetGame().GetClientManager().GetNumberById(Buddy.UserId);
                        L.Add(new ResultStructure { Username = Buddy.mUsername, Look = Buddy.mLook , PhoneNumber  = Number});
                    }
                }

                PacketData = JsonConvert.SerializeObject(L);
            }
        }

        private class ResultStructure
        {
            public string Username { get; set; }
            public string Look { get; set; }
            public string PhoneNumber { get; set; }
        }

        private class ErrorStructure
        {
            public string Error { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
        }
    }
}
