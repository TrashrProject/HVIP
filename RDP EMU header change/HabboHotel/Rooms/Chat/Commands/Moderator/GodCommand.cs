using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;

using Plus.Database.Interfaces;
using Plus.HabboHotel.Users.Effects;


namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    class GodCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_god"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Acivas inmunidad God/Dias"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Session.GetPlay().PassiveMode)
            {
                Session.SendWhisper("No puedes hacer eso mientras estás en modo pasivo", 1);
                return;
            }

            User.GetClient().GetPlay().IsGodMode = !User.GetClient().GetPlay().IsGodMode;

            if (User.GetClient().GetPlay().IsGodMode)
            {
                User.GetClient().GetPlay().GodMode = true;
                User.GetClient().GetPlay().FirstTickBool = false;
                User.GetClient().GetPlay().GodModeTicks = 0;
                User.GetClient().GetRoomUser().ApplyEffect(EffectsList.Fireflies);
                Session.SendWhisper("¡Inmunidad God activado!", 1);
            }
            else
            {
                User.GetClient().GetPlay().GodMode = false;
                User.GetClient().GetPlay().FirstTickBool = false;
                User.GetClient().GetPlay().GodModeTicks = 0;
                User.GetClient().GetRoomUser().ApplyEffect(EffectsList.None);
                Session.SendWhisper("¡Inmunidad God desactivado!", 1);
            }
        }
    }
}
