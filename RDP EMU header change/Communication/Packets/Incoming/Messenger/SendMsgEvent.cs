using System;
using System.Linq;
using System.Text;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Messenger
{
    class SendMsgEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            bool active = false;
            if (!active)
                return;

            int userId = Packet.PopInt();

            string message = StringCharFilter.Escape(Packet.PopString());

            //this is for staff chat
            if (userId == -0x7fffffff)
            {

                PlusEnvironment.GetGame().GetClientManager().StaffAlert(new NewConsoleMessageComposer(-0x7fffffff, message, Session.GetHabbo().Username + "/" + Session.GetHabbo().Look + "/" + Session.GetHabbo().Id), Session.GetHabbo().Id);
                return;
            }

            //this is for group chat
            else if(userId < 0)
            {
                Group thegroup;
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(-userId, out thegroup))
                    return;

                if (!thegroup.GroupChatEnabled)
                    return;

                if (!thegroup.IsMember(Session.GetHabbo().Id))
                    return;

                List<GameClient> GroupMembers = (from Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList() where Client != null && Client.GetHabbo() != null && thegroup.IsMember(Client.GetHabbo().Id) select Client).ToList();
                foreach (GameClient Client in GroupMembers)
                {
                    if (Session.GetHabbo().Username == Client.GetHabbo().Username)
                        continue;
                    if (Client == null)
                        continue;
                    Client.SendMessage(new NewConsoleMessageComposer(-thegroup.Id, message, Session.GetHabbo().Username + "/" + Session.GetHabbo().Look + "/" + Session.GetHabbo().Id));
                }

                
                return;
            }


            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(message))
            {
                PlusEnvironment.GetGame().GetClientManager().StaffAlert(new RoomNotificationComposer("Alerta de Publicidad",
                              "El Usuario: <b>" + Session.GetHabbo().Username + "<br>" +

                              "<br></b> Esta Publicando y/o usando una palabra contenida en el filtro " + "<br>" +
                              "<br></b> Mediante : <b> Chat Por Consola <br>" +
                              "<br><b>La Palabra Usada Fue:</b><br>" +
                                "<br>" + "<b>" + "<font color =\"#06087F\">" + message + "</font>" + "</b><br>" +
                              "<br>Para ir a la sala, da clic en \"Ir a la Sala \"</b>",
                              "filter", "Ir a la Sala", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                message = "Estoy Intentando Publicar Otro Hotel Porfavor Advierte a un Staff";
            }


            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendNotification("Oops, you're currently muted - you cannot send messages.");
                return;
            }

            Session.GetHabbo().GetMessenger().SendInstantMessage(userId, message);

        }
    }
}