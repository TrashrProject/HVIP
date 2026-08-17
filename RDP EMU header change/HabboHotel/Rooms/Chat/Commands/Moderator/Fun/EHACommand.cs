using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using System;
namespace Plus.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class EHACommand : IChatCommand
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
                return "Send a hotel alert for your event!";
            }
        }
        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (Session != null)
            {
                if (Room != null)
                {
                    string Message = "" +  "Hey there's a event going on right now you might wanna head down there!";
                    if (Params.Length > 2)
                        Message = CommandManager.MergeParams(Params, 1);

                    PlusEnvironment.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("Evento en marcha", Message + "\r\n- <b>" + Session.GetHabbo().Username + "</b>\r\n<i></i>", "figure/" + Session.GetHabbo().Username + "", "Go to \"" + Session.GetHabbo().CurrentRoom.Name + "\"!", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                }
            }
        }
    }
}