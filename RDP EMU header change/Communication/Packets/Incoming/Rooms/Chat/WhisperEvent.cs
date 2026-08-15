using System;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Core;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    public class WhisperEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;



            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Room.CheckMute(Session))
            {
                Session.SendWhisper("Oops, you're currently muted.");
                return;
            }

            if (PlusEnvironment.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                return;



            string Params = Packet.PopString();
            string ToUser = Params.Split(' ')[0];
            string Message = Params.Substring(ToUser.Length + 1);
            int Colour = Packet.PopInt();

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            RoomUser User2 = Room.GetRoomUserManager().GetRoomUserByHabbo(ToUser);
            if (User2 == null)
                return;

            if (Session.GetHabbo().TimeMuted > 0)
            {
                Session.SendMessage(new MutedComposer(Session.GetHabbo().TimeMuted));
                return;
            }
            



            ChatStyle Style = null;
            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(Colour, out Style) || (Style.RequiredRight.Length > 0 && !Session.GetHabbo().GetPermissions().HasRight(Style.RequiredRight)))
                Colour = 0;

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

            if (!User2.GetClient().GetHabbo().ReceiveWhispers && !Session.GetHabbo().GetPermissions().HasRight("room_whisper_override"))
            {
                Session.SendWhisper("Oops, this user has their whispers disabled!");
                return;
            }

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatlog(new Plus.HabboHotel.Rooms.Chat.Logs.ChatlogEntry(Session.GetHabbo().Id, Room.Id, "<Whisper to " + ToUser + ">: " + Message, UnixTimestamp.GetNow(), Session.GetHabbo(), Room));

            Room.AddChatlog(Session.GetHabbo().Id, "<Susurra a " + ToUser + ">: " + Message);

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

                        Session.GetHabbo().GetClient().SendMessage(new WhisperComposer(User.VirtualId, Session.GetHabbo().BannedPhraseCount + "/" + PlusStaticGameSettings.BannedPhrasesAmount + " Estas usando palabras prohibidas. Seras baneado si continuas.", 0, 34));
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

            User.UnIdle();
            User.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));

            if (User2 != null && !User2.IsBot && User2.UserId != User.UserId)
            {
                if (!User2.GetClient().GetHabbo().MutedUsers.Contains(Session.GetHabbo().Id))
                {
                    User2.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, User.LastBubble));
                }
            }

            List<RoomUser> ToNotify = Room.GetRoomUserManager().GetRoomUserByRank(3);
            if (ToNotify.Count > 0)
            {
                foreach (RoomUser user in ToNotify)
                {
                    if (user != null && user.HabboId != User2.HabboId && user.HabboId != User.HabboId)
                    {
                        if (user.GetClient() != null && user.GetClient().GetHabbo() != null && !user.GetClient().GetHabbo().IgnorePublicWhispers)
                        {
                            user.GetClient().SendMessage(new WhisperComposer(User.VirtualId, "[Susurra a " + ToUser + "] " + Message, 0, User.LastBubble));
                        }
                    }
                }
            }
        }
    }
}