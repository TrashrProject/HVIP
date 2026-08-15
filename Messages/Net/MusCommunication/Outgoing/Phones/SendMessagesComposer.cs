
using Newtonsoft.Json;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboRoleplay.PhoneChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Plus.Messages.Net.MusCommunication.Outgoing.Phones
{
    class SendMessagesComposer : MusPacketEvent
    {
        // ID can be APPID or WebID (Check if user have acces to this ID)
        public SendMessagesComposer(int ID = 0, int UserID = 0)
        {
            List<ErrorStructure> E = new List<ErrorStructure>();
            E.Add(new ErrorStructure { Error = "true", Code = "5000", Message = "No se encontraron datos para enviar." });

            // Incoming Information to send to Client
            PacketName = "event_sendmessages";
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

                #region Execute
                string ChatRooms = "";
                List<PhoneChat> Chats = PlusEnvironment.GetGame().GetPhoneChatManager().GetPhoneChatsByMyID(Client.GetHabbo().Id);

                // Si tiene chats guardados.
                if (Chats != null && Chats.Count > 0)
                {
                    #region ChatRooms
                    // Almacenamos ChatRooms en SendData
                    Chats.Reverse();
                    foreach (PhoneChat PChats in Chats)
                    {
                        int ChatUsId = PChats.EmisorId;
                        string ChatName = PChats.EmisorName;

                        // Si el receptor es difenrete a mí, el emisor soy yo...
                        if (PChats.ReceptorName != Client.GetHabbo().Username)
                        {
                            ChatName = PChats.ReceptorName;
                            ChatUsId = PChats.ReceptorId;
                        }
                        // Guardamos los nombres de Chats existentes
                        ChatRooms += ChatName + ";";

                        // Variable para el encabezado visual del chat
                        string DisplayName = ChatName;

                        // Verificamos si el chat es un Amigo
                        List<MessengerBuddy> Friend = (from TG in Client.GetHabbo().GetMessenger().GetFriends().ToList() where TG != null && TG.mUsername == ChatName select TG).ToList();
                        if (Friend == null || Friend.Count <= 0)
                        {
                            // Si no está en la lista de amigos buscamos su número.
                            DisplayName = PlusEnvironment.GetGame().GetClientManager().GetNumberById(ChatUsId);
                        }

                        // Spliteamos los chatrooms almacenados
                        string[] stringSeparators = new string[] { ";" };
                        string[] result;
                        int count = 0;
                        result = ChatRooms.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string inf in result)
                        {
                            if (inf == ChatName)
                                count++;
                        }
                        if (count <= 1)
                        {
                            string msg = PChats.Msg;
                            // Si el emisor soy yo, anteponer 'Yo:'
                            if (PChats.EmisorName == Client.GetHabbo().Username)
                                msg = "Yo: " + PChats.Msg;

                            L.Add(new ResultStructure { ChatName = ChatName, DisplayName = DisplayName, Time = Convert.ToString(PChats.TimeStamp), Msg = Regex.Replace(msg, "::br::", " ") });

                            // Verifica si se quedó en un Chat específico abierto
                            /*
                            if (Client.GetPlay().LastChat == ChatName)
                            {
                                Client.GetPlay().UpdateChats = true;
                                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "open_messages," + ChatName);
                                Client.GetPlay().UpdateChats = false;
                            }
                            */
                        }
                    }
                    #endregion
                }
                #endregion
                PacketData = JsonConvert.SerializeObject(L);
            }
        }

        private class ResultStructure
        {
            public string ChatName { get; set; }
            public string DisplayName { get; set; }
            public string Time { get; set; }
            public string Msg { get; set; }
        }

        private class ErrorStructure
        {
            public string Error { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
        }
    }
}
