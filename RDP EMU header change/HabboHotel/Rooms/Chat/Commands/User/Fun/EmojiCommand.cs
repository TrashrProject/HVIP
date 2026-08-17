using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboRoleplay.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class EmojiCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_emoji"; }
        }
        public string Parameters
        {
            get { return "%EmojiId%"; }
        }
        public string Description
        {
            get { return "Número de 1-189. Manda un emoji"; }
        }
        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("¡Oops, debes escribir un número entre 1 y 189! ((Para ver la lista de emojis usa :emoji list))", 1);
                return;
            }
            string emoji = Params[1];

           if(emoji.Equals("list"))
            {
                Session.SendMessage(new NuxAlertMessageComposer("habbopages/chat/emoji.txt"));
            }
            else
            {
                int emojiNum;
                bool isNumeric = int.TryParse(emoji, out emojiNum);
                if (isNumeric)
                {
                    string chatcolor = Session.GetHabbo().chatHTMLColour;
                    int chatsize = Session.GetHabbo().chatHTMLSize;

                    Session.GetHabbo().chatHTMLColour = "";
                    Session.GetHabbo().chatHTMLSize = 12;
                    switch (emojiNum)
                    {
                        default:
                            bool isValid = true;
                            if (emojiNum < 1)
                            {
                                isValid = false;
                            }

                            if (emojiNum > 189 && Session.GetHabbo().Rank < 6)
                            {
                                isValid = false;
                            }
                            if (isValid)
                            {
                                string Username;
                                RoomUser TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
                                if (emojiNum < 10)
                                {
                                    Username = "<img src='"+ RoleplayManager.CDNSWF + "/c_images/emoji/Emoji_Smiley/Emoji%20Smiley-0" + emojiNum + ".png' height='20' width='20'><br>    ";
                                }
                                else
                                {
                                    Username = "<img src='" + RoleplayManager.CDNSWF + "/c_images/emoji/Emoji_Smiley/Emoji%20Smiley-" + emojiNum + ".png' height='20' width='20'><br>    ";
                                }
                                if (Room != null)
                                    Room.SendMessage(new UserNameChangeComposer(Session.GetHabbo().CurrentRoomId, TargetUser.VirtualId, Username));

                                string Message = " ";
                                Room.SendMessage(new ChatComposer(TargetUser.VirtualId, Message, 0, TargetUser.LastBubble));
                                TargetUser.SendNamePacket();

                            }
                            else
                            {
                                Session.SendWhisper("Emoji inválido, debe ser un número entre 1 y 189. ((Para ver la lista de emojis usa :emoji list))", 1);
                            }

                            break;
                    }
                    Session.GetHabbo().chatHTMLColour = chatcolor;
                    Session.GetHabbo().chatHTMLSize = chatsize;
                }
                else
                {
                    Session.SendWhisper("Emoji inválido, debe ser un número entre 1 y 189. ((Para ver la lista de emojis usa :emoji list))", 1);
                }
            }
            return;
        }
    }
}
