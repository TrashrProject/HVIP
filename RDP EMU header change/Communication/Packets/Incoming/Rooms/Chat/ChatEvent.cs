using System;

using Plus.Core;
using Plus.Communication.Packets.Incoming;
using Plus.Utilities;
using Plus.HabboHotel.Global;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Rooms.Chat.Styles;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    public class ChatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session == null || Session.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            string Message = StringCharFilter.Escape(Packet.PopString());
            if (Message.Length > 100)
                Message = Message.Substring(0, 100);

            int Colour = Packet.PopInt();

            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) || (Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight)))
                Colour = 0;

            User.UnIdle();

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }


            if (!Session.GetHabbo().GetPermissions().HasRight("room_ignore_mute") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Oops, you're currently muted.");
                return;
            }

            User.LastBubble = Session.GetHabbo().CustomBubbleId == 0 ? Colour : Session.GetHabbo().CustomBubbleId;

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                int MuteTime;
                if (User.IncrementAndCheckFlood(out MuteTime))
                {
                    Session.SendMessage(new FloodControlComposer(MuteTime));
                    return;
                }
            }

            if (Message.StartsWith(":", StringComparison.CurrentCulture) && PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(Session, Message))
                return;

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatlog(new ChatlogEntry(Session.GetHabbo().Id, Room.Id, Message, UnixTimestamp.GetNow(), Session.GetHabbo(), Room));

            Room.AddChatlog(Session.GetHabbo().Id, Message);

            if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(Message) && !Session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
            {

                PlusEnvironment.GetGame().GetClientManager().StaffAlert(new RoomNotificationComposer("Alerta de Publicidad",
                             "El Usuario: <b>" + Session.GetHabbo().Username + "<br>" +

                             "<br></b> Esta Publicando y/o usando una palabra contenida en el filtro" + "<br>" +

                             "<br><b>La Palabra Usada Fue:</b><br>" +
                               "<br>" + "<b>" + "<font color =\"#FF0000\">" + Message + "</font>" + "</b><br>" +
                             "<br>Para ir a la sala, da clic en \"Ir a la Sala \"</b>",
                             "filter", "Ir a la Sala", "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                if (PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckBannedWords(Message))
                {
                    Session.GetHabbo().BannedPhraseCount++;
                   

                    if (Session.GetHabbo().BannedPhraseCount >= PlusStaticGameSettings.BannedPhrasesAmount)
                    {
                        PlusEnvironment.GetGame().GetModerationManager().BanUser("System", HabboHotel.Moderation.ModerationBanType.USERNAME, Session.GetHabbo().Username, "Por mandar spam con palabras prohibidas (" + Message + ")", (PlusEnvironment.GetUnixTimestamp() + 78892200));
                        Session.Disconnect();
                        return;
                    }
                    else
                    {
                        
                        Session.GetHabbo().GetClient().SendMessage(new WhisperComposer(User.VirtualId, Session.GetHabbo().BannedPhraseCount + "/" + PlusStaticGameSettings.BannedPhrasesAmount +" Estas usando palabras prohibidas. Seras baneado si continuas.", 0, 34));
                    }
                }
                else
                {
                    Session.GetHabbo().GetClient().SendMessage(new WhisperComposer(User.VirtualId, "La siguiente palabra esta prohibida en el hotel:" + " " + Message, 0, 34));

                }


                Message = null;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("word_filter_override") && Message != null)
                Message = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Message);


            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.SOCIAL_CHAT);

            User.OnChat(User.LastBubble, Message, false);
        }
    }
}