using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Rooms.Games.Teams;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    class EnableCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_enable"; }
        }

        public string Parameters
        {
            get { return "%EffectId%"; }
        }

        public string Description
        {
            get { return "Activa un efecto en tu personaje."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes especificar un ID.", 1);
                return;
            }

            if (!Room.EnablesEnabled && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Al parecer no se puede hacer eso aquí. La zona no tiene enables activos o tú no tienes permisos.", 1);
                return;
            }

            RoomUser ThisUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);
            if (ThisUser == null)
                return;

            if (ThisUser.RidingHorse)
            {
                Session.SendWhisper("No puedes hacer eso mientras montas un caballo.", 1);
                return;
            }
            else if (ThisUser.Team != TEAM.NONE)
                return;
            else if (ThisUser.isLying)
                return;

            int EffectId = 0;
            if (!int.TryParse(Params[1], out EffectId))
                return;

            if (EffectId > int.MaxValue || EffectId < int.MinValue)
                return;

            if ((EffectId == 102 || EffectId == 187) && !Session.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                Session.SendWhisper("Solo miembros del Staff pueden usar este efecto.", 1);
                return;
            }
            /*
            if (EffectId == 178 && (!Session.GetHabbo().GetPermissions().HasRight("gold_vip") && !Session.GetHabbo().GetPermissions().HasRight("events_staff")))
            {
                Session.SendWhisper("Solo miembros del equipo de ayudantes pueden hacer uso de este efecto.", 1);
                return;
            }
            */
            Session.GetHabbo().Effects().ApplyEffect(EffectId);
        }
    }
}
