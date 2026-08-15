using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using System;
namespace Plus.HabboHotel.Rooms.Chat.Commands.Events
{//publialert
    internal class PublicityAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_publi_alert";
            }
        }
        public string Parameters
        {
            get { return ""; }
        }
        public string Description
        {
            get
            {
                return "Envía una alerta para invitar a oleadas de publicidad.";
            }
        }
        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session == null) return;
            if (Room == null) return;
            PlusEnvironment.GetGame().GetClientManager().SendMessage(new SuperNotificationComposer("events", "¡Nueva oleada en este momento!", "La oleada ha sido abierta por: <b><font color='#FE2EF7'>" + Session.GetHabbo().Username + " </font></b>\n¡Asiste a ella para ganar premios, placas y conocer nuevos amigos! ¿QUÉ ESPERAS?" + "\r\rLas oleadas de publicidad, colaboran con el servidor para crecer juntos como comunidad.\n\n",
                "Ir a la Oleada", "event:navigator/goto/" + Room.Id));
        }
    }
}