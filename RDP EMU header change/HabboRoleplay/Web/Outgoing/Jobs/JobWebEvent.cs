using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using System.IO;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Incoming.Groups;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Messenger;
using System.Collections.Generic;
using Plus.HabboHotel.Groups;
using System.Text.RegularExpressions;

namespace Plus.HabboHotel.Roleplay.Web.Outgoing.Misc
{
    /// <summary>
    /// JobWebEvent class.
    /// </summary>
    class JobWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {

            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;
            /*
            if (!Client.GetPlay().UsingAtm)
            {
                Client.SendNotification("Buen intento, tratando de injectar el systema, ve a un ATM!");
                return;
            }
            */

            string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

            //Generamos la Sala
            Room Room = RoleplayManager.GenerateRoom(Client.GetRoomUser().RoomId);

            if(Room.Group == null)
            {
                Client.SendNotification("Esta zona no es ningún punto de solicitud de Trabajo.");
                return;
            }

            switch (Action)
            {

                #region Open
                case "open":
                    {                     
                        string Founder = "Ninguno";
                       
                        if (Room.Group.GetAdministrator.Count > 0)
                            Founder = PlusEnvironment.GetUsernameById(Room.Group.GetAdministrators[0]);
                        
                        int Pay = GroupManager.GetGroupRank(Room.Group.Id, 1).Pay;

                        string SendData = "";
                        SendData += Room.Group.Name + ",";
                        SendData += Room.Group.Description + ",";
                        SendData += "$" + Pay.ToString() + " cada 10 minutos,";
                        SendData += Room.Group.Badge + ",";
                        SendData += Founder;
                        Socket.Send("compose_job:open:" + SendData);
                    }
                    break;
                #endregion

                #region Close
                case "close":
                    {
                        Client.GetPlay().JobRequest = false;
                        break;
                    }
                #endregion

                #region Send
                case "send":
                    {
                        #region Vars
                        string[] ReceivedData = Data.Split(',');
                        int time;
                        string textwork = Convert.ToString(ReceivedData[1]);
                        string zone = Convert.ToString(ReceivedData[3]);

                        // FILTER
                        textwork = Regex.Replace(textwork, "<(.|\\n)*?>", string.Empty);
                        zone = Regex.Replace(zone, "<(.|\\n)*?>", string.Empty);
                        #endregion

                        #region Conditions
                        if (!int.TryParse(ReceivedData[2], out time))
                        {
                            Socket.Send("compose_job:error:Debes ingresar un número (entero) de tus horas libres.");
                            return;
                        }

                        if (time < 0)
                        {
                            Socket.Send("compose_job:error:Debes ingresar un número (entero y positivo) de tus horas libres.");
                            return;
                        }

                        if (textwork.Length < 10)
                        {
                            Socket.Send("compose_job:error:Debes ingresar un mínimo de 10 caracteres en el campo de texto.");
                            return;
                        }

                        if (zone == "" || zone == null)
                        {
                            Socket.Send("compose_job:error:Debes seleccionar tu País de residencia.");
                            return;
                        }

                        if (Client.GetPlay().TryGetCooldown("jobrequest"))
                            return;
                        #endregion

                        #region Execute

                        RoleplayManager.Shout(Client, "*Envía una solicitud de empleo con el mensaje '" + textwork + "' es de " + zone + " y tiene " + time + " horas libres al día*", 5);
                        Client.GetPlay().JobRequest = true;
                        Socket.Send("compose_job:close");
                        Client.GetPlay().CooldownManager.CreateCooldown("jobrequest", 1000, 30);

                        #region Execute Packet
                        Room.Group.AddMember(Client.GetHabbo().Id);

                        if (Room.Group.GroupType == GroupType.LOCKED)
                        {
                            List<GameClient> GroupAdmins = (from Session in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && Room.Group.IsAdmin(Client.GetHabbo().Id) select Client).ToList();
                            foreach (GameClient Session in GroupAdmins)
                            {
                                Session.SendMessage(new GroupMembershipRequestedComposer(Room.Group.Id, Session.GetHabbo(), 3));
                            }

                            Client.SendMessage(new GroupInfoComposer(Room.Group, Client));
                        }
                        else
                        {
                            Client.SendMessage(new GroupFurniConfigComposer(PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(Client.GetHabbo().Id)));
                            Client.SendMessage(new GroupInfoComposer(Room.Group, Client));

                            if (Client.GetHabbo().CurrentRoom != null)
                                Client.GetHabbo().CurrentRoom.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));
                            else
                                Client.SendMessage(new RefreshFavouriteGroupComposer(Client.GetHabbo().Id));

                            if (Room.Group.GroupChatEnabled)
                                Client.SendMessage(new FriendListUpdateComposer(-Room.Group.Id, Room.Group.Id));
                        }
                        Client.GetPlay().JobRequest = false;
                        #endregion

                        #endregion
                    }
                    break;
                #endregion
            }
        }
    }
}
