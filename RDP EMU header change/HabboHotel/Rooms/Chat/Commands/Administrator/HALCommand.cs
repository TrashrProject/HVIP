using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    class HALCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_hal"; }
        }

        public string Parameters
        {
            get { return "%message% %URL%"; }
        }

        public string Description
        {
            get { return "Envía una alerta a todo el servidor con una URL."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 2)
            {
                Session.SendWhisper("Debes ingresar un mensaje y una URL.", 1);
                return;
            }

            string URL = Params[1];

            string Message = CommandManager.MergeParams(Params, 2);
            PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("Alerta del Servidor", Message + "\r\n" + "- " + Session.GetHabbo().Username, "", URL, URL));
            return;
        }
    }
}
