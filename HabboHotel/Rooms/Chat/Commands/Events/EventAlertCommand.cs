using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Events
{//eventalert
    internal class EventAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get
            {
                return "command_event_alert";
            }
        }
        public string Parameters
        {
            get
            {
                return "%message%";
            }
        }
        public string Description
        {
            get
            {
                return "¡Envia una alerta para tu evento!";
            }
        }
        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session != null)
            {
                if (Room != null)
                {
                    if (Params.Length == 1)
                    {
                        Session.SendWhisper("Por favor, digita un mensaje para enviar.", 1);
                        return;
                    }
                    else
                    {
                        string Message = CommandManager.MergeParams(Params, 1);

                        PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("¡Hay un nuevo evento!",
                            "¡Hay un nuevo Evento en este momento! Si quieres ganar premios, rares, duckets  y placas, este es tu momento." + "<br>" +

                             "<br>Se trata de un evento abierto por: <b> " + "<font color =\"#600FCF\">" + Session.GetHabbo().Username + "</font>" + "<br>" +

                             "</b>¡Asiste a el para ganar premios, placas y conocer nuevos amigos! ¿QUÉ ESPERAS? Da clic en <b><br> ¡Participar! </b>." + "<br>" +

                             "<br>¿De qué trata el evento?<br>" +
                               "<br>" + "<b>" + "<font color =\"#0F89CF\">" + Message + "</font>" + "</b><br>" +
                             "<br>¡Te esperamos!</b>",
                             "events", "¡Participar!", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                    }
                }
            }
        }

    }
}