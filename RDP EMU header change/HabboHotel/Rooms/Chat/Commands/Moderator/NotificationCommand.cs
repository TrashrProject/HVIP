using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    class NotificationCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_notification"; }
        }

        public string Parameters
        {
            get { return "%message%"; }
        }

        public string Description
        {
            get { return "Manda una notificación a todo el servidor."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes escribir un mensaje.", 1);
                return;
            }


            string message = CommandManager.MergeParams(Params, 1);
            //Session.SendMessage(new RoomNotificationComposer("notification.gifting.valentine", "message", value));
            //furni_placement_error
            //(string image, int messageType, string message, string link)
            PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("ADM", 3, message, "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
            return;
        }
    }
}
