using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    class MakeSayCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_makesay"; }
        }

        public string Parameters
        {
            get { return "%username% %message%"; }
        }

        public string Description
        {
            get { return "Forza a una persona a decir un mensaje."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            if (Params.Length == 1)
                Session.SendWhisper("Debes escribir un mensaje.", 1);
            else
            {
                string Message = CommandManager.MergeParams(Params, 2);
                RoomUser TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
                if (TargetUser != null)
                {
                    if (TargetUser.GetClient() != null && TargetUser.GetClient().GetHabbo() != null)
                        if (!TargetUser.GetClient().GetHabbo().GetPermissions().HasRight("mod_make_say_any"))
                            Room.SendMessage(new ChatComposer(TargetUser.VirtualId, Message, 0, TargetUser.LastBubble));
                        else
                            Session.SendWhisper("No puedes hacer eso con esta persona.", 1);
                }
                else
                    Session.SendWhisper("No se encontró a esa persona en esta zona.", 1);
            }
        }
    }
}
