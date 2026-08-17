using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;
using System;
namespace Plus.HabboHotel.Rooms.Chat.Commands.Events
{//da2alert
    internal class DiceAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_da2_alert";
            }
        }
        public string Parameters
        {
            get { return "%Message%"; }
        }
        public string Description
        {
            get
            {
                return "Send a hotel alert for your event!";
            }
        }
        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session == null) return;
            if (Room == null) return;
            PlusEnvironment.GetGame().GetClientManager().SendMessage(new SuperNotificationComposer("da2alert", "¡Se han abierto los dados oficiales!", "El inter que abre los dados es: <b><font color='#FF8000'>" + Session.GetHabbo().Username + " </font></b>\nA diferencia de los dados comunes, es que en estos puedes apostar con total seguridad" + "\r\rLos inters serán los encargados de supervisar que todo se realiza de manera correcta\n\n ¡¿A QUE ESPERAS?! ¡Ven ya y gana apostando contra otros usuarios!",
                "Ir a la sala", "event:navigator/goto/" + Room.Id));
            
        }
    }
}