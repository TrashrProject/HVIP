
using Newtonsoft.Json;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.PhoneChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Plus.Messages.Net.MusCommunication.Outgoing.Phones
{
    class SendMessageComposer : MusPacketEvent
    {
        // ID can be APPID or WebID (Check if user have acces to this ID)
        public SendMessageComposer(GameClient Client, string Target = null, string Text = null)
        {
            List<ErrorStructure> E = new List<ErrorStructure>();
            E.Add(new ErrorStructure { Error = "true", Code = "5000", Message = "No se encontraron datos para enviar." });

            // Incoming Information to send to Client
            PacketName = "event_sendmessage";
            PacketData = JsonConvert.SerializeObject(E);

            List<ResultStructure> L = new List<ResultStructure>();

            #region Conditons
            // Filtramos
            //Text = Regex.Replace(Text, "<(.|\\n)*?>", string.Empty); <- Por emojis
            Target = Regex.Replace(Target, "<(.|\\n)*?>", string.Empty);

            if (Target.Length <= 0)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "in_app_error,<b>Ha ocurrido un error</b><br>No puedes enviar mensajes sin destinatario.|");
                return;
            }
            if (Text.Length <= 0)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "in_app_error,<b>Ha ocurrido un error</b><br>No puedes enviar mensajes sin texto.|");
                return;
            }
            #endregion

            /*
                * El Target recibido puede ser
                * número telefónico, ya sea con o sin formato
                * ó el nombre de usuario.
                * Si es nombre de usuario éste debe ser "amigo"
                * Nota: Validar cuando el Target esté online o no.
            */

            int Targetid = PlusEnvironment.GetGame().GetPhoneChatManager().GetIDbyContact(Client, Target);

            #region Extra Conditions
            if (Targetid <= 0)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "in_app_error,<b>Ha ocurrido un error</b><br>Ese número telefónico no existe.|");
                return;
            }
            if (Targetid == Client.GetHabbo().Id)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "in_app_error,<b>Ha ocurrido un error</b><br>No puedes enviarte mensajes a ti mism@.|");
                return;
            }

            // Envió a un nombre de contacto
            if (Client.GetPlay().SendToName)
            {
                // Verificamos que sean amigos
                List<MessengerBuddy> Friend = (from TG in Client.GetHabbo().GetMessenger().GetFriends().ToList() where TG != null && TG.UserId == Targetid select TG).ToList();

                if (Friend == null || Friend.Count <= 0)
                {
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Client, "event_phone", "in_app_error,<b>Ha ocurrido un error</b><br>No se encontró ese nombre en tus Contactos. Intenta escribiendo el Número Telefónico.|");
                    return;
                }
            }
            #endregion

            #region Execute
            int ID = RoleplayManager.ChatsID += 1;
            DateTime TimeStamp = DateTime.Now;
            string TargetName = PlusEnvironment.GetUserInfoBy("username", "id", Convert.ToString(Targetid));

            // TryAdd al Diccionario de PhoneChat
            PlusEnvironment.GetGame().GetPhoneChatManager().NewPhoneChat(ID, 1, Client.GetHabbo().Id, Client.GetHabbo().Username, Targetid, TargetName, Text, TimeStamp);

            // Comprobamos si destinatario está online
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Targetid);
            if (TargetClient != null)
            {
                PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_phone", "open_chatrooms");
                RoleplayManager.Shout(TargetClient, "*Recibe un Mensaje de Texto*", 5);

                // Verificamos si el último chat del destinatario es el nuestro y actualizamos
                if (TargetClient.GetPlay().LastChat == Client.GetHabbo().Username)
                {
                    // Refresh ChatRooms Target
                    TargetClient.GetPlay().UpdateChats = true;
                    PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_phone", "open_messages," + Client.GetHabbo().Username);
                    TargetClient.GetPlay().UpdateChats = false;

                    // Refresh Msgs Target
                    if (TargetClient.GetPlay().LastChat == Client.GetHabbo().Username)
                        PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(TargetClient, "event_phone", "open_chatrooms");
                }
            }


            RoleplayManager.Shout(Client, "*Ha enviado un Mensaje de Texto*", 5);
            Client.GetPlay().CooldownManager.CreateCooldown("msg", 1000, 3);
            #endregion


            L.Add(new ResultStructure { Status = "TRUE", TargetName = TargetName });
            PacketData = JsonConvert.SerializeObject(L);
        }

        private class ResultStructure
        {
            public string Status { get; set; }
            public string TargetName { get; set; }
        }

        private class ErrorStructure
        {
            public string Error { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
        }
    }
}
